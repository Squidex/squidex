/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { debug, DialogService, LoadingState, shareSubscribed, State } from '@app/framework';
import { ScriptLogDto } from '../model';
import { ScriptLogsService } from '../services/script-logs.service';
import { AppsState } from './apps.state';

interface Snapshot extends LoadingState {
    // The current script logs.
    scriptLogs: ReadonlyArray<ScriptLogDto>;

    // The name or name prefix of the scripts.
    name?: string;
}

@Injectable({
    providedIn: 'root',
})
export class ScriptLogsState extends State<Snapshot> {
    public scriptLogs =
        this.project(x => x.scriptLogs);

    public name =
        this.project(x => x.name);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public get appName() {
        return this.appsState.appName;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly scriptLogsService: ScriptLogsService,
        private readonly dialogs: DialogService,
    ) {
        super({ scriptLogs: [] });

        debug(this, 'scriptLogs');
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState('Loading Initial');
        }

        return this.loadInternal(isReload);
    }

    public filter(name?: string): Observable<any> {
        this.next({ name: name?.trim() || undefined }, 'Filtered');

        return this.loadInternal(false);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        this.next({ isLoading: true }, 'Loading Started');

        return this.scriptLogsService.getScriptLogs(this.appName, this.snapshot.name).pipe(
            tap(({ items: scriptLogs }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:scriptLogs.reloaded');
                }

                this.next({
                    scriptLogs,
                    isLoaded: true,
                    isLoading: false,
                }, 'Loading Success');
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs));
    }
}
