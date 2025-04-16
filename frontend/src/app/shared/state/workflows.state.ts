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

        for (const [stepName, step] of Object.entries(dto.steps)) {
            const { transitions: _, ...values } = step.toJSON();
            const stepView = { name: stepName, isLocked: stepName === 'Published', values };

            if (step.transitions) {
                for (const [to, transition] of Object.entries(step.transitions)) {
                    const asJson = transition.toJSON();

                    resultTransitions.push({ from: stepName, to, step: stepView, values: asJson });
                }
            }

            resultSteps.push(stepView);
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
        const clone = this.dto.toJSON();

        clone.steps[name] = { transitions: {}, ...clone.steps[name] ?? {}, ...values };
        if (!clone.initial) {
            clone.initial = name;
        }

        return new WorkflowView(WorkflowDto.fromJSON(clone));
    }

    public removeStep(name: string) {
        const step = this.steps.find(x => x.name === name);
        if (!step || step.isLocked) {
            return this;
        }

        const clone = this.dto.toJSON();

        delete clone.steps[name];
        if (clone.initial === name) {
            clone.initial = this.steps.filter(x => x.name !== name && !x.isLocked)[0]?.name ?? null;
        }

        for (const step of Object.values(clone.steps) as any[]) {
            delete step.transitions[name];
        }

        return new WorkflowView(WorkflowDto.fromJSON(clone));
    }

    public changeSchemaIds(schemaIds: string[]) {
        if (this.dto.schemaIds === schemaIds) {
            return this;
        }

        const clone = this.dto.toJSON();
        clone.schemaIds = schemaIds;
        return new WorkflowView(WorkflowDto.fromJSON(clone));
    }

    public changeName(name: string) {
        if (this.dto.name === name) {
            return this;
        }

        const clone = this.dto.toJSON();
        clone.name = name;
        return new WorkflowView(WorkflowDto.fromJSON(clone));
    }

    public setInitial(initial: string) {
        const step = this.dto.steps[initial];
        if (!step) {
            return this;
        }

        if (this.dto.initial === initial) {
            return this;
        }

        const clone = this.dto.toJSON();
        clone.initial = initial;
        return new WorkflowView(WorkflowDto.fromJSON(clone));
    }

    public setTransition(from: string, to: string, values: Partial<WorkflowTransitionValues> = {}) {
        const step = this.dto.steps[from];
        if (!step) {
            return this;
        }

        const clone = this.dto.toJSON();
        clone.steps[from].transitions[to] = { transitions: {}, ...clone.steps[from].transitions[to] ?? {}, ...values };
        return new WorkflowView(WorkflowDto.fromJSON(clone));
    }

    public removeTransition(from: string, to: string) {
        const step = this.dto.steps[from];
        if (!step) {
            return this;
        }

        const transition = step.transitions![to];
        if (!transition) {
            return this;
        }

        const clone = this.dto.toJSON();
        delete clone.steps[from].transitions[to];
        return new WorkflowView(WorkflowDto.fromJSON(clone));
    }

    public renameStep(name: string, newName: string) {
        const step = this.dto.steps[name];
        if (!step) {
            return this;
        }

        const clone = this.dto.toJSON();

        clone.steps[newName] = clone.steps[name];
        delete clone.steps[name];

        for (const step of Object.values(clone.steps) as any[]) {
            if (step.transitions[name]) {
                step.transitions[newName] = step.transitions[name];
                delete step.transitions[name];
            }
        }

        return new WorkflowView(WorkflowDto.fromJSON(clone));
    }
}