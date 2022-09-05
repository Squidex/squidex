/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiUrlConfig, compareStrings, hasAnyLink, HTTP, mapVersioned, pretifyError, Resource, ResourceLinks, StringHelper, Version, Versioned } from '@app/framework';

export class WorkflowDto {
    public readonly _links: ResourceLinks;

    public readonly canDelete: boolean;
    public readonly canUpdate: boolean;

    public readonly displayName: string;

    constructor(links: ResourceLinks,
        public readonly id: string,
        public readonly name: string | null = null,
        public readonly initial: string | null = null,
        public readonly schemaIds: string[] = [],
        public readonly steps: WorkflowStep[] = [],
        public readonly transitions: WorkflowTransition[] = [],
    ) {
        this._links = links;

        this.canDelete = hasAnyLink(links, 'delete');
        this.canUpdate = hasAnyLink(links, 'update');

        this.displayName = StringHelper.firstNonEmpty(name, 'i18n:workflows.notNamed');
    }

    protected onCloned() {
        this.steps.sort((a, b) => {
            return compareStrings(a.name, b.name);
        });

        this.transitions.sort((a, b) => {
            return compareStrings(a.to, b.to);
        });
    }

    public getOpenSteps(step: WorkflowStep) {
        return this.steps.filter(x => x.name !== step.name && !this.transitions.find(y => y.from === step.name && y.to === x.name));
    }

    public getTransitions(step: WorkflowStep): WorkflowTransitionView[] {
        return this.transitions.filter(x => x.from === step.name).map(x => ({ step: this.getStep(x.to), ...x }));
    }

    public getStep(name: string): WorkflowStep {
        return this.steps.find(x => x.name === name)!;
    }

    public setStep(name: string, values: Partial<WorkflowStepValues> = {}) {
        const old = this.getStep(name);

        const step = { ...old, name, ...values };
        const steps = [...this.steps.filter(s => s !== old), step];

        if (steps.length === 1) {
            return this.with({ initial: name, steps });
        } else {
            return this.with({ steps });
        }
    }

    public setTransition(from: string, to: string, values: Partial<WorkflowTransitionValues> = {}) {
        if (!this.getStep(from) || !this.getStep(to)) {
            return this;
        }

        const old = this.transitions.find(x => x.from === from && x.to === to);

        const transition = { ...old, from, to, ...values };
        const transitions = [...this.transitions.filter(t => t !== old), transition];

        return this.with({ transitions });
    }

    public setInitial(initial: string) {
        const found = this.getStep(initial);

        if (!found || found.isLocked) {
            return this;
        }

        return this.with({ initial });
    }

    public removeStep(name: string) {
        const steps = this.steps.filter(s => s.name !== name || s.isLocked);

        if (steps.length === this.steps.length) {
            return this;
        }

        const transitions = this.transitions.filter(t => t.from !== name && t.to !== name);

        if (this.initial === name) {
            const first = steps.find(x => !x.isLocked);

            return this.with({ initial: first?.name || null, steps, transitions });
        } else {
            return this.with({ steps, transitions });
        }
    }

    public changeSchemaIds(schemaIds: string[]) {
        return this.with({ schemaIds });
    }

    public changeName(name: string) {
        return this.with({ name });
    }

    public renameStep(name: string, newName: string) {
        const steps = this.steps.map(step => {
            if (step.name === name) {
                return { ...step, name: newName };
            }

            return step;
        });

        const transitions = this.transitions.map(transition => {
            if (transition.from === name || transition.to === name) {
                const newTransition = { ...transition };

                if (newTransition.from === name) {
                    newTransition.from = newName;
                }

                if (newTransition.to === name) {
                    newTransition.to = newName;
                }

                return newTransition;
            }

            return transition;
        });

        if (this.initial === name) {
            return this.with({ initial: newName, steps, transitions });
        } else {
            return this.with({ steps, transitions });
        }
    }

    public removeTransition(from: string, to: string) {
        const transitions = this.transitions.filter(t => t.from !== from || t.to !== to);

        if (transitions.length === this.transitions.length) {
            return this;
        }

        return this.with({ transitions });
    }

    public serialize(): any {
        const result = { steps: {}, schemaIds: this.schemaIds, initial: this.initial, name: this.name };

        for (const step of this.steps) {
            const { name, ...values } = step;

            const s = { ...values, transitions: {} };

            for (const transition of this.getTransitions(step)) {
                const { to, step: _, from: __, ...t } = transition;

                s.transitions[to] = t;
            }

            result.steps[name] = s;
        }

        return result;
    }

    private with(update: Partial<WorkflowDto>) {
        const clone = Object.assign(Object.assign(Object.create(Object.getPrototypeOf(this)), this), update);

        clone.onCloned();

        return clone;
    }
}

export type WorkflowsDto = Versioned<WorkflowsPayload>;

export type WorkflowsPayload = Readonly<{
    // The list of workflows.
    items: WorkflowDto[];

    // The validations errors.
    errors: string[];

    // True, if the user has permissions to create a new workflow.
    canCreate?: boolean;
}>;

export type WorkflowStepValues = Readonly<{
    // The color of the step.
    color?: string;

    // True, if the step cannot be removed.
    isLocked?: boolean;

    // True, if the content should be validated on this step.
    validate?: boolean;

    // True, when the step has an update restriction.
    noUpdate?: boolean;

    // The expression when updates are not allowed.
    noUpdateExpression?: string;

    // The user roles which cannot update a content.
    noUpdateRoles?: ReadonlyArray<string>;
}>;

export type WorkflowStep = Readonly<{
    // The name of the workflow.
    name: string;
} & WorkflowStepValues>;

export type WorkflowTransitionValues = Readonly<{
    // The expression when a transition is possible.
    expression?: string;

    // The user roles which can transition to this step.
    roles?: string[];
}>;

export type WorkflowTransition = Readonly<{
    // The source step name.
    from: string;

    // The target step name.
    to: string;
} & WorkflowTransitionValues>;

export type WorkflowTransitionView = Readonly<{
    // The actual workflow step.
    step: WorkflowStep;
} & WorkflowTransition>;

export type CreateWorkflowDto = Readonly<{
    // The name of the workflow.
    name: string;
}>;

@Injectable()
export class WorkflowsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
    ) {
    }

    public getWorkflows(appName: string): Observable<WorkflowsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/workflows`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return parseWorkflows(body);
            }),
            pretifyError('i18n:workflows.loadFailed'));
    }

    public postWorkflow(appName: string, dto: CreateWorkflowDto, version: Version): Observable<WorkflowsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/workflows`);

        return HTTP.postVersioned(this.http, url, dto, version).pipe(
            mapVersioned(({ body }) => {
                return parseWorkflows(body);
            }),
            pretifyError('i18n:workflows.createFailed'));
    }

    public putWorkflow(appName: string, resource: Resource, dto: any, version: Version): Observable<WorkflowsDto> {
        const link = resource._links['update'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version, dto).pipe(
            mapVersioned(({ body }) => {
                return parseWorkflows(body);
            }),
            pretifyError('i18n:workflows.updateFailed'));
    }

    public deleteWorkflow(appName: string, resource: Resource, version: Version): Observable<WorkflowsDto> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            mapVersioned(({ body }) => {
                return parseWorkflows(body);
            }),
            pretifyError('i18n:workflows.deleteFailed'));
    }
}

function parseWorkflows(response: { items: any[]; errors: string[] } & Resource) {
    const { items: list, errors, _links } = response;
    const items = list.map(parseWorkflow);

    const canCreate = hasAnyLink(_links, 'create');

    return { items, errors, canCreate };
}

function parseWorkflow(workflow: any) {
    const { id, name, initial, schemaIds, _links } = workflow;

    const resultSteps: WorkflowStep[] = [];
    const resultTransitions: WorkflowTransition[] = [];

    for (const [stepName, stepValue] of Object.entries(workflow.steps)) {
        const { transitions, ...step } = stepValue as any;

        resultSteps.push({ name: stepName, isLocked: stepName === 'Published', ...step });

        for (const [to, transition] of Object.entries(transitions)) {
            resultTransitions.push({ from: stepName, to, ...transition as any });
        }
    }

    return new WorkflowDto(_links, id, name, initial, schemaIds, resultSteps, resultTransitions);
}
