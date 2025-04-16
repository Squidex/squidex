/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { compareStrings, debug, DialogService, LoadingState, shareSubscribed, State, VersionTag } from '@app/framework';
import { WorkflowsService } from '../services/workflows.service';
import { IUpdateWorkflowDto, IWorkflowStepDto, IWorkflowTransitionDto, WorkflowDto, WorkflowsDto } from './../model';
import { AppsState } from './apps.state';

interface Snapshot extends LoadingState {
    // The current workflow.
    workflows: ReadonlyArray<WorkflowDto>;

    // The app version.
    version: VersionTag;

    // The errors.
    errors: ReadonlyArray<string>;

    // Indicates if the user can create new workflow.
    canCreate?: boolean;
}

@Injectable({
    providedIn: 'root',
})
export class WorkflowsState extends State<Snapshot> {
    public workflows =
        this.project(x => x.workflows);

    public errors =
        this.project(x => x.errors);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    public get appId() {
        return this.appsState.appId;
    }

    public get appName() {
        return this.appsState.appName;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly workflowsService: WorkflowsService,
    ) {
        super({ errors: [], workflows: [], version: VersionTag.EMPTY });

        debug(this, 'workflows');
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState('Loading Initial');
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        this.next({ isLoading: true });

        return this.workflowsService.getWorkflows(this.appName).pipe(
            tap(({ version, payload }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:workflows.reloaded');
                }

                this.replaceWorkflows(payload, version);
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs));
    }

    public add(name: string): Observable<any> {
        return this.workflowsService.postWorkflow(this.appName, { name }, this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceWorkflows(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    public update(workflow: WorkflowDto, update: IUpdateWorkflowDto): Observable<any> {
        return this.workflowsService.putWorkflow(this.appName, workflow, update, this.version).pipe(
            tap(({ version, payload }) => {
                this.dialogs.notifyInfo('i18n:workflows.saved');

                this.replaceWorkflows(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    public delete(workflow: WorkflowDto): Observable<any> {
        return this.workflowsService.deleteWorkflow(this.appName, workflow, this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceWorkflows(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    private replaceWorkflows(payload: WorkflowsDto, version: VersionTag) {
        const { canCreate, errors, items: workflows } = payload;

        this.next({
            canCreate,
            errors,
            isLoaded: true,
            isLoading: false,
            version,
            workflows,
        }, 'Loading Success / Updated');
    }

    private get version() {
        return this.snapshot.version;
    }
}

export interface WorkflowStepView {
    // The name of the workflow.
    name: string;

    // The actual step.
    values: WorkflowStepValues;

     // True, if the step cannot be removed.
     isLocked?: boolean;
}

export type WorkflowStepValues = Omit<IWorkflowStepDto, 'transitions'>;

export interface WorkflowTransitionView {
    // The source step name.
    from: string;

    // The target step name.
    to: string;

    // The actual transition.
    values: WorkflowTransitionValues;

    // The actual workflow step.
    step: WorkflowStepView;
}

export type WorkflowTransitionValues = IWorkflowTransitionDto;

export class WorkflowView {
    public steps: ReadonlyArray<WorkflowStepView> = [];

    public transitions: ReadonlyArray<WorkflowTransitionView> = [];

    constructor(
        public readonly dto: WorkflowDto,
    ) {
        const resultSteps: WorkflowStepView[] = [];
        const resultTransitions: WorkflowTransitionView[] = [];

        for (const [name, step] of Object.entries(dto.steps)) {
            const { transitions: _, ...values } = step.toJSON();
            
            resultSteps.push({ name, isLocked: isLocked(name), values });
        }

        for (const [from, step] of Object.entries(dto.steps)) {
            if (step.transitions) {
                for (const [to, transition] of Object.entries(step.transitions)) {
                    const step = resultSteps.find(x => x.name === to)!;

                    resultTransitions.push({ from, to, step, values: transition.toJSON() });
                }
            }
        }

        this.steps =
            resultSteps.sort((a, b) => {
                return compareStrings(a.name, b.name);
            });

        this.transitions =
            resultTransitions.sort((a, b) => {
                return compareStrings(a.to, b.to);
            });
    }

    public getOpenSteps(step: WorkflowStepView) {
        return this.steps.filter(x => x.name !== step.name && !this.transitions.find(y => y.from === step.name && y.to === x.name));
    }

    public getTransitions(step: WorkflowStepView): WorkflowTransitionView[] {
        return this.transitions.filter(x => x.from === step.name);
    }

    public getStep(name: string): WorkflowStepView {
        return this.steps.find(x => x.name === name)!;
    }

    public setStep(name: string, values: Partial<WorkflowStepValues> = {}) {
        return this.update(clone => {
            clone.steps[name] = { transitions: {}, ...clone.steps[name] ?? {}, ...values };

            if (!clone.initial && !isLocked(name)) {
                clone.initial = name;
            }
        });
    }

    public removeStep(name: string) {
        const step = this.steps.find(x => x.name === name);
        if (!step || step.isLocked) {
            return this;
        }
        
        return this.update(clone => {
            delete clone.steps[name];

            if (clone.initial === name) {
                clone.initial = this.steps.filter(x => x.name !== name && !x.isLocked)[0]?.name ?? null;
            }

            for (const step of Object.values(clone.steps) as any[]) {
                delete step.transitions[name];
            }
        });
    }

    public changeSchemaIds(schemaIds: string[]) {
        if (this.dto.schemaIds === schemaIds) {
            return this;
        }

        return this.update(clone => {
            clone.schemaIds = schemaIds;
        });
    }

    public changeName(name: string) {
        if (this.dto.name === name) {
            return this;
        }

        return this.update(clone => {
            clone.name = name;
        });
    }

    public setInitial(initial: string) {
        const step = this.steps.find(x => x.name === initial);
        if (!step || step.isLocked || this.dto.initial === initial) {
            return this;
        }

        return this.update(clone => {
            clone.initial = initial;
        });
    }

    public setTransition(from: string, to: string, values: Partial<WorkflowTransitionValues> = {}) {
        if (!this.dto.steps[from] || !this.dto.steps[to]) {
            return this;
        }

        return this.update(clone => {
            clone.steps[from].transitions[to] = { transitions: {}, ...clone.steps[from].transitions[to] ?? {}, ...values };
        });
    }

    public removeTransition(from: string, to: string) {
        const step = this.dto.steps[from];
        if (!step || !step.transitions?.[to]) {
            return this;
        }

        return this.update(clone => {
            delete clone.steps[from].transitions[to];
        });
    }

    public renameStep(name: string, newName: string) {
        if (!this.dto.steps[name] || name === newName) {
            return this;
        }

        return this.update(clone => {
            renameInObj(clone.steps, name, newName);
    
            for (const step of Object.values(clone.steps) as any[]) {
                renameInObj(step.transitions, name, newName);
            }
        });
    }

    private update(action: (clone: any) => void) {
        const clone = this.dto.toJSON();
        action(clone);
        return new WorkflowView(WorkflowDto.fromJSON(clone));
    }
}

function renameInObj(target: any, name: string, newName: string) {
    const existing = target[name];
    if (existing) {
        target[newName] = existing;
        delete target[name];
    }
}

function isLocked(name: string) {
    return name === 'Published';
}