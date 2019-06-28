/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import {
    AnalyticsService,
    ApiUrlConfig,
    compareStringsAsc,
    hasAnyLink,
    HTTP,
    mapVersioned,
    pretifyError,
    Model,
    Resource,
    ResourceLinks,
    Version,
    Versioned
} from '@app/framework';

export type WorkflowsDto = Versioned<WorkflowsPayload>;
export type WorkflowsPayload = {
    readonly items: WorkflowDto[];

    readonly canCreate: boolean;
} & Resource;

export class WorkflowDto extends Model<WorkflowDto> {
    public readonly _links: ResourceLinks;

    public readonly canUpdate: boolean;

    public static DEFAULT =
        new WorkflowDto()
            .setStep('Draft', { color: '#8091a5' })
            .setStep('Archived', { color: '#eb3142', noUpdate: true })
            .setStep('Published', { color: '#4bb958', isLocked: true })
            .setTransition('Archived', 'Draft')
            .setTransition('Draft', 'Archived')
            .setTransition('Draft', 'Published')
            .setTransition('Published', 'Draft')
            .setTransition('Published', 'Archived');

    constructor(links: ResourceLinks = {},
        public readonly initial?: string,
        public readonly steps: WorkflowStep[] = [],
        private readonly transitions: WorkflowTransition[] = []
    ) {
        this.steps.sort((a, b) => compareStringsAsc(a.name, b.name));

        this.transitions.sort((a, b) => compareStringsAsc(a.to, b.to));

        this._links = links;

        this.canUpdate = hasAnyLink(links, 'update');
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
        const found = this.getStep(name);

        if (found) {
            const { name: _, ...existing } = found;

            if (found.isLocked) {
                return this;
            }

            values = { ...existing,  ...values };
        }

        const steps = [...this.steps.filter(s => s !== found), { name, ...values }];

        let initial = this.initial;

        if (steps.length === 1) {
            initial = steps[0].name;
        }

        return new WorkflowDto(this._links, initial, steps, this.transitions);
    }

    public setInitial(initial: string) {
        const found = this.getStep(initial);

        if (!found || found.isLocked) {
            return this;
        }

        return new WorkflowDto(this._links, initial, this.steps, this.transitions);
    }

    public removeStep(name: string) {
        const steps = this.steps.filter(s => s.name !== name || s.isLocked);

        if (steps.length === this.steps.length) {
            return this;
        }

        const transitions =
            steps.length !== this.steps.length ?
                this.transitions.filter(t => t.from !== name && t.to !== name) :
                this.transitions;

        let initial = this.initial;

        if (initial === name) {
            const first = steps.find(x => !x.isLocked);

            initial = first ? first.name : undefined;
        }

        return new WorkflowDto(this._links, initial, steps, transitions);
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
                let newTransition = { ...transition };

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

        let initial = this.initial;

        if (initial === name) {
            initial = newName;
        }

        return new WorkflowDto(this._links, initial, steps, transitions);
    }

    public removeTransition(from: string, to: string) {
        const transitions = this.transitions.filter(t => t.from !== from || t.to !== to);

        if (transitions.length === this.transitions.length) {
            return this;
        }

        return new WorkflowDto(this._links, this.initial, this.steps, transitions);
    }

    public setTransition(from: string, to: string, values: Partial<WorkflowTransitionValues> = {}) {
        const stepFrom = this.getStep(from);

        if (!stepFrom) {
            return this;
        }

        const stepTo = this.getStep(to);

        if (!stepTo) {
            return this;
        }

        const found = this.transitions.find(x => x.from === from && x.to === to);

        if (found) {
            const { from: _, to: __, ...existing } = found;

            values = { ...existing, ...values };
        }

        const transitions = [...this.transitions.filter(t => t !== found), { from, to, ...values }];

        return new WorkflowDto(this._links, this.initial, this.steps, transitions);
    }

    public serialize(): any {
        const result = { steps: {}, initial: this.initial };

        for (let step of this.steps) {
            const { name, ...values } = step;

            const s = { ...values, transitions: {} };

            for (let transition of this.getTransitions(step)) {
                const { from, to, step: _, ...t } = transition;

                s.transitions[to] = t;
            }

            result.steps[name] = s;
        }

        return result;

    }
}

export type WorkflowStepValues = { color?: string; isLocked?: boolean; noUpdate?: boolean; };
export type WorkflowStep = { name: string } & WorkflowStepValues;

export type WorkflowTransitionValues = { expression?: string; role?: string; };
export type WorkflowTransition = { from: string; to: string } & WorkflowTransitionValues;

export type WorkflowTransitionView = { step: WorkflowStep } & WorkflowTransition;

@Injectable()
export class WorkflowsService {
    constructor(
        private readonly http: HttpClient,
        private readonly apiUrl: ApiUrlConfig,
        private readonly analytics: AnalyticsService
    ) {
    }

    public getWorkflows(appName: string): Observable<WorkflowsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/workflows`);

        return HTTP.getVersioned(this.http, url).pipe(
            mapVersioned(({ body }) => {
                return parseWorkflow(body);
            }),
            pretifyError('Failed to load workflows. Please reload.'));
    }

    public postWorkflows(appName: string, dto: WorkflowDto, version: Version): Observable<WorkflowsDto> {
        const url = this.apiUrl.buildUrl(`api/apps/${appName}/workflows`);

        return HTTP.postVersioned(this.http, url, dto, version).pipe(
            mapVersioned(({ body }) => {
                return parseWorkflow(body);
            }),
            tap(() => {
                this.analytics.trackEvent('Workflow', 'Configured', appName);
            }),
            pretifyError('Failed to add Workflow. Please reload.'));
    }

    public deleteWorkflow(appName: string, resource: Resource, version: Version): Observable<WorkflowsDto> {
        const link = resource._links['delete'];

        const url = this.apiUrl.buildUrl(link.href);

        return HTTP.requestVersioned(this.http, link.method, url, version).pipe(
            mapVersioned(payload => {
                const body = payload.body;

                return parseWorkflow(body);
            }),
            tap(() => {
                this.analytics.trackEvent('Workflow', 'Deleted', appName);
            }),
            pretifyError('Failed to delete Workflow. Please reload.'));
    }
}

function parseWorkflow(response: any) {
    const raw: any[] = response.items;

    const items = raw.map(item =>
        new WorkflowDto(item._links,
            item._name,
            item._steps,
            item._transitions));

    const { _links, _meta } = response;

    return { items, _links, _meta, canCreate: hasAnyLink(_links, 'create') };
}
export type WorkflowTransitionView = { step: WorkflowStep } & WorkflowTransition;

@Injectable()
export class WorkflowsService {
    public getWorkflow(appName: string): Observable<Versioned<WorkflowPayload>> {
        return of(versioned(new Version('1'), { workflow: WorkflowDto.DEFAULT, _links: {} }));
    }

    public putWorkflow(appName: string, resource: Resource, dto: any, version: Version): Observable<Versioned<WorkflowPayload>> {
        return of(versioned(new Version('1'), { workflow: WorkflowDto.DEFAULT, _links: {} }));
    }
}