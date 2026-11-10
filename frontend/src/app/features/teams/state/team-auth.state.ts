/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize, map, tap } from 'rxjs/operators';
import { AuthSchemeDto, AuthSchemeResponseDto, AuthSchemeValueDto, debug, DialogService, LoadingState, shareSubscribed, State, TeamsService, TeamsState, VersionTag } from '@app/shared';

interface Snapshot extends LoadingState {
    // The current scheme.
    scheme?: AuthSchemeDto | null;

    // The current response, needed for the links.
    response?: AuthSchemeResponseDto;

    // Indicates if the user can update the auth settings.
    canUpdate?: boolean;

    // Indicates if the user can verify the domain.
    canVerify?: boolean;

    // Indicates if the domain is verified.
    isVerified?: boolean;

    // The name of the DNS record to verify the domain.
    verificationRecordName?: string;

    // The value of the DNS record to verify the domain.
    verificationRecordValue?: string;

    // The team version.
    version: VersionTag;
}

@Service({ autoProvided: false })
export class TeamAuthState extends State<Snapshot> {
    private readonly dialogs = inject(DialogService);
    private readonly teamService = inject(TeamsService);
    private readonly teamsState = inject(TeamsState);

    public readonly scheme =
        this.project(x => x.scheme);

    public readonly canUpdate =
        this.project(x => x.canUpdate === true);

    public readonly canVerify =
        this.project(x => x.canVerify === true);

    public readonly isVerified =
        this.project(x => x.isVerified === true);

    public readonly verificationRecordName =
        this.project(x => x.verificationRecordName);

    public readonly verificationRecordValue =
        this.project(x => x.verificationRecordValue);

    public readonly isLoaded =
        this.project(x => x.isLoaded === true);

    public readonly isLoading =
        this.project(x => x.isLoading === true);

    public get teamId() {
        return this.teamsState.teamId;
    }

    constructor() {
        super({ version: VersionTag.EMPTY });

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

                this.replacePayload(payload, version, { isLoaded: true, isLoading: false }, 'Loading Success');
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs));
    }

    public update(scheme: AuthSchemeDto | undefined): Observable<AuthSchemeDto | undefined | null> {
        return this.teamService.putTeamAuth(this.teamId, AuthSchemeValueDto.fromJSON({ scheme }), this.version).pipe(
            tap(({ version, payload }) => {
                this.replacePayload(payload, version, {}, 'Change');
            }),
            map(({ payload }) => {
                return payload.scheme;
            }),
            shareSubscribed(this.dialogs));
    }

    public verify(): Observable<any> {
        const response = this.snapshot.response;
        if (!response) {
            throw new Error('Auth settings not loaded.');
        }

        return this.teamService.verifyTeamAuth(response, this.version).pipe(
            tap(({ version, payload }) => {
                this.dialogs.notifyInfo('i18n:teams.auth.verified');

                this.replacePayload(payload, version, {}, 'Verified');
            }),
            shareSubscribed(this.dialogs));
    }

    private replacePayload(payload: AuthSchemeResponseDto, version: VersionTag, state: Partial<Snapshot>, event: string) {
        const { canUpdate, canVerify, isVerified, scheme, verificationRecordName, verificationRecordValue } = payload;

        this.next({
            ...state,
            canUpdate,
            canVerify,
            isVerified,
            response: payload,
            scheme,
            verificationRecordName,
            verificationRecordValue,
            version,
        }, event);
    }

    private get version() {
        return this.snapshot.version;
    }
}
