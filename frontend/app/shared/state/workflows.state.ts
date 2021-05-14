/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { DialogService, shareSubscribed, State, Version } from '@app/framework';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { WorkflowDto, WorkflowsPayload, WorkflowsService } from './../services/workflows.service';
import { AppsState } from './apps.state';

interface Snapshot {
    // The current workflow.
    workflows: ReadonlyArray<WorkflowDto>;

    // The app version.
    version: Version;

    // The errors.
    errors: ReadonlyArray<string>;

    // Indicates if the workflows are loaded.
    isLoaded?: boolean;

    // Indicates if the workflows are loading.
    isLoading?: boolean;

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
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    constructor(
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly workflowsService: WorkflowsService,
    ) {
        super({ errors: [], workflows: [], version: Version.EMPTY }, 'Workflows');
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

    public update(workflow: WorkflowDto): Observable<any> {
        return this.workflowsService.putWorkflow(this.appName, workflow, workflow.serialize(), this.version).pipe(
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

    private replaceWorkflows(payload: WorkflowsPayload, version: Version) {
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

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }
}
