/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { EMPTY, Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { debug, DialogService, LoadingState, Resource, shareSubscribed, State } from '@app/framework';
import { JobDto } from '../model';
import { JobsService } from '../services/jobs.service';
import { AppsState } from './apps.state';

interface Snapshot extends LoadingState {
    // The current jobs.
    jobs: ReadonlyArray<JobDto>;

    // Indicates if the user can create new backups.
    canCreateBackup?: boolean;

    // The resource to rebuild the full text index, if the user is allowed to do so.
    textIndexResource?: Resource;
}

@Injectable({
    providedIn: 'root',
})
export class JobsState extends State<Snapshot> {
    public jobs =
        this.project(x => x.jobs);

    public maxBackupsReached =
        this.projectFrom(this.jobs, x => x.filter(j => j.taskName === 'backup').length >= 10);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canCreateBackup =
        this.project(x => x.canCreateBackup === true);

    public canRebuildTextIndex =
        this.project(x => !!x.textIndexResource);

    public get appId() {
        return this.appsState.appId;
    }

    public get appName() {
        return this.appsState.appName;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly jobsService: JobsService,
        private readonly dialogs: DialogService,
    ) {
        super({ jobs: [] });

        debug(this, 'jobs');
    }

    public load(isReload = false, silent = false): Observable<any> {
        if (isReload && !silent) {
            this.resetState('Loading Initial');
        }

        return this.loadInternal(isReload, silent);
    }

    private loadInternal(isReload: boolean, silent: boolean): Observable<any> {
        if (!silent) {
            this.next({ isLoading: true }, 'Loading Started');
        }

        return this.jobsService.getJobs(this.appName).pipe(
            tap(payload => {
                const { items: jobs, canCreateBackup, canRebuildTextIndex } = payload;

                if (isReload && !silent) {
                    this.dialogs.notifyInfo('i18n:jobs.reloaded');
                }

                this.next({
                    jobs,
                    canCreateBackup,
                    textIndexResource: canRebuildTextIndex ? payload : undefined,
                    isLoaded: true,
                    isLoading: false,
                }, 'Loading Success');
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs, { silent }));
    }

    public startBackup(): Observable<any> {
        return this.jobsService.postBackup(this.appsState.appName).pipe(
            tap(() => {
                this.dialogs.notifyInfo('i18n:jobs.started');
            }),
            shareSubscribed(this.dialogs));
    }

    public rebuildTextIndex(): Observable<any> {
        const resource = this.snapshot.textIndexResource;

        if (!resource) {
            return EMPTY;
        }

        return this.jobsService.postTextIndexRebuild(this.appsState.appName, resource).pipe(
            tap(() => {
                this.dialogs.notifyInfo('i18n:jobs.started');
            }),
            shareSubscribed(this.dialogs));
    }

    public delete(job: JobDto): Observable<any> {
        return this.jobsService.deleteJob(this.appsState.appName, job).pipe(
            tap(() => {
                this.dialogs.notifyInfo('i18n:jobs.deleted');
            }),
            shareSubscribed(this.dialogs));
    }
}
