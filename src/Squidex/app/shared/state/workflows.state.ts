/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: no-shadowed-variable

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import {
    DialogService,
    ImmutableArray,
    shareSubscribed,
    State,
    Version
} from '@app/framework';

import { AppsState } from './apps.state';

import {
    WorkflowDto,
    WorkflowsPayload,
    WorkflowsService
} from './../services/workflows.service';

interface Snapshot {
    // The current workflow.
    workflows: ImmutableArray<WorkflowDto>;

    // The app version.
    version: Version;

    // The errors.
    errors: string[];

    // Indicates if the workflows are loaded.
    isLoaded?: boolean;

    // Indicates if the user can create new workflow.
    canCreate?: boolean;
}

@Injectable()
export class WorkflowsState extends State<Snapshot> {
    public workflows =
        this.project(x => x.workflows);

    public errors =
        this.project(x => x.errors);

    public isLoaded =
        this.project(x => !!x.isLoaded);

    public canCreate =
        this.project(x => !!x.canCreate);

    constructor(
        private readonly workflowsService: WorkflowsService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService
    ) {
        super({ errors: [], workflows: ImmutableArray.empty(), version: Version.EMPTY });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.workflowsService.getWorkflows(this.appName).pipe(
            tap(({ version, payload }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Workflows reloaded.');
                }

                this.replaceWorkflows(payload, version);
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

    public update(workflow: WorkflowDto): Observable<any> {
        return this.workflowsService.putWorkflow(this.appName, workflow, workflow.serialize(), this.version).pipe(
            tap(({ version, payload }) => {
                this.dialogs.notifyInfo('Workflow has been saved.');

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

    private replaceWorkflows(payload: WorkflowsPayload, version: Version) {
        const { canCreate, errors, items } = payload;

        const workflows = ImmutableArray.of(items);

        this.next(s => {
            return { ...s, workflows, errors, isLoaded: true, version, canCreate };
        });
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }
}