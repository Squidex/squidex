/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { compareStrings, debug, DialogService, LoadingState, Mutable, shareSubscribed, State, VersionTag } from '@app/framework';
import { WorkflowsService } from '../services/workflows.service';
import { AddWorkflowDto, IWorkflowStepDto, IWorkflowTransitionDto, UpdateWorkflowDto, WorkflowDto, WorkflowsDto, WorkflowStepDto, WorkflowTransitionDto } from './../model';
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

    public add(request: AddWorkflowDto): Observable<any> {
        return this.workflowsService.postWorkflow(this.appName, request, this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceWorkflows(payload, version);
            }),
            shareSubscribed(this.dialogs));
    }

    public update(workflow: WorkflowDto, request: UpdateWorkflowDto): Observable<any> {
        return this.workflowsService.putWorkflow(this.appName, workflow, request, this.version).pipe(
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
            clone.steps[name] = new WorkflowStepDto({ transitions: {}, ...clone.steps[name] ?? {}, ...values });

            if (!clone.initial && !isLocked(name)) {
                clone.initial = name;
            }

            return true;
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

            return true;
        });
    }

    public changeSchemaIds(schemaIds: string[]) {
        if (this.dto.schemaIds === schemaIds) {
            return this;
        }

        return this.update(clone => {
            clone.schemaIds = schemaIds;
            return true;
        });
    }

    public changeName(name: string) {
        if (this.dto.name === name) {
            return this;
        }

        return this.update(clone => {
            clone.name = name;
            return true;
        });
    }

    public setInitial(initial: string) {
        const step = this.steps.find(x => x.name === initial);
        if (!step || step.isLocked || this.dto.initial === initial) {
            return this;
        }

        return this.update(clone => {
            clone.initial = initial;
            return true;
        });
    }

    public setTransition(from: string, to: string, values: Partial<WorkflowTransitionValues> = {}) {
        return this.update(clone => {
            const fromStep = clone.steps[from] as Mutable<WorkflowStepDto>;
            if (!fromStep || !clone.steps[to]) {
                return false;
            }

            fromStep.transitions ??= {};
            fromStep.transitions[to] = new WorkflowTransitionDto({ ...fromStep.transitions[to] ?? {}, ...values });
            return true;
        });
    }

    public removeTransition(from: string, to: string) {
        return this.update(clone => {
            const fromStep = clone.steps[from] as Mutable<WorkflowStepDto>;
            if (!fromStep?.transitions?.[to]) {
                return false;
            }

            delete clone.steps[from].transitions![to];
            return true;
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

            return true;
        });
    }

    private update(action: (clone: Mutable<WorkflowDto>) => boolean) {
        const clone = WorkflowDto.fromJSON(this.dto.toJSON());
        if (!action(clone)) {
            return this;
        }
        return new WorkflowView(clone);
    }

    public toUpdate(): UpdateWorkflowDto {
        return UpdateWorkflowDto.fromJSON(this.dto.toJSON());
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