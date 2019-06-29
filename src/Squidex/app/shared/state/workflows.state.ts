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
    shareMapSubscribed,
    shareSubscribed,
    State,
    Version
} from '@app/framework';

import { AppsState } from './apps.state';

import {
    WorkflowDto,
    WorkflowPayload,
    WorkflowsService
} from './../services/workflows.service';

interface Snapshot {
    // The current workflow.
    workflow?: WorkflowDto;

    // The app version.
    version: Version;

    // Indicates if the workflows are loaded.
    isLoaded?: boolean;
}

@Injectable()
export class WorkflowsState extends State<Snapshot> {
    public workflow =
        this.project(x => x.workflow);

    public isLoaded =
        this.project(x => !!x.isLoaded);

    constructor(
        private readonly workflowsService: WorkflowsService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService
    ) {
        super({ version: Version.EMPTY });
    }

    public load(isReload = false): Observable<WorkflowDto> {
        if (!isReload) {
            this.resetState();
        }

        return this.workflowsService.getWorkflow(this.appName).pipe(
            tap(({ version, payload }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Workflow reloaded.');
                }

                this.replaceWorkflow(payload, version);
            }),
            shareMapSubscribed(this.dialogs, x => x.payload.workflow));
    }

    public save(workflow: WorkflowDto): Observable<any> {
        return this.workflowsService.putWorkflow(this.appName, workflow, workflow.serialize(), this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceWorkflow(payload, version);

                this.dialogs.notifyInfo('Workflow has been saved.');
            }),
            shareSubscribed(this.dialogs));
    }

    private replaceWorkflow(payload: WorkflowPayload, version: Version) {
        const { workflow } = payload;

        this.next(s => {
            return { ...s, workflow, isLoaded: true, version };
        });
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }
}