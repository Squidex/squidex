/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize, map, tap } from 'rxjs/operators';
import { AuthSchemeDto, debug, DialogService, LoadingState, shareSubscribed, State, TeamsService, TeamsState, Version } from '@app/shared';

interface Snapshot extends LoadingState {
    // The current scheme.
    scheme?: AuthSchemeDto | null;

    // Indicates if the user can update the auth settings.
    canUpdate?: boolean;

    // The team version.
    version: Version;
}

@Injectable({
    providedIn: 'any',
})
export class TeamAuthState extends State<Snapshot> {
    public scheme =
        this.project(x => x.scheme);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public get teamId() {
        return this.teamsState.teamId;
    }

    constructor(
        private readonly teamsState: TeamsState,
        private readonly dialogs: DialogService,
        private readonly teamService: TeamsService,
    ) {
        super({ version: Version.EMPTY });

        debug(this, 'teamAuth');
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState('Loading Initial');
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        this.next({ isLoading: true }, 'Loading Started');

        return this.teamService.getTeamAuth(this.teamId).pipe(
            tap(({ version, payload }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:teams.auth.reloaded');
                }

                this.next({
                    ...payload,
                    isLoaded: true,
                    isLoading: false,
                    version,
                }, 'Loading Success');
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs));
    }

    public update(scheme: AuthSchemeDto | null): Observable<AuthSchemeDto | undefined | null> {
        return this.teamService.putTeamAuth(this.teamId, { scheme }, this.version).pipe(
            tap(({ payload, version }) => {
                this.next({
                    ...payload,
                    version,
                }, 'Change');
            }),
            map(({ payload }) => {
                return payload.scheme;
            }),
            shareSubscribed(this.dialogs));
    }

    private get version() {
        return this.snapshot.version;
    }
}