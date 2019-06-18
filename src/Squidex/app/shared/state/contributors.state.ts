/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

import {
    DialogService,
    ErrorDto,
    ImmutableArray,
    shareMapSubscribed,
    shareSubscribed,
    State,
    Types,
    Version
} from '@app/framework';

import {
    AssignContributorDto,
    ContributorDto,
    ContributorsPayload,
    ContributorsService
} from './../services/contributors.service';

import { AppsState } from './apps.state';

interface Snapshot {
    // All loaded contributors.
    contributors: ContributorsList;

    // Indicates if the contributors are loaded.
    isLoaded?: boolean;

    // The maximum allowed users.
    maxContributors: number;

    // The app version.
    version: Version;

    // Indicates if the user can add a contributor.
    canCreate?: boolean;
}

type ContributorsList = ImmutableArray<ContributorDto>;

@Injectable()
export class ContributorsState extends State<Snapshot> {
    public contributors =
        this.project(x => x.contributors);

    public isLoaded =
        this.project(x => !!x.isLoaded);

    public maxContributors =
        this.project(x => x.maxContributors);

    public canCreate =
        this.project(x => !!x.canCreate);

    constructor(
        private readonly contributorsService: ContributorsService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService
    ) {
        super({ contributors: ImmutableArray.empty(), version: Version.EMPTY, maxContributors: -1 });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.contributorsService.getContributors(this.appName).pipe(
            tap(({ version, payload }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Contributors reloaded.');
                }

                this.replaceContributors(version, payload);
            }),
            shareSubscribed(this.dialogs));
    }

    public revoke(contributor: ContributorDto): Observable<any> {
        return this.contributorsService.deleteContributor(this.appName, contributor, this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceContributors(version, payload);
            }),
            shareSubscribed(this.dialogs));
    }

    public assign(request: AssignContributorDto): Observable<boolean | undefined> {
        return this.contributorsService.postContributor(this.appName, request, this.version).pipe(
            catchError(error => {
                if (Types.is(error, ErrorDto) && error.statusCode === 404) {
                    return throwError(new ErrorDto(404, 'The user does not exist.'));
                } else {
                    return throwError(error);
                }
            }),
            tap(({ version, payload }) => {
                this.replaceContributors(version, payload);
            }),
            shareMapSubscribed(this.dialogs, x => x.payload._meta && x.payload._meta['isInvited'] === '1'));
    }

    private replaceContributors(version: Version, payload: ContributorsPayload) {
        this.next(s => {
            const { canCreate, items, maxContributors } = payload;

            const contributors = ImmutableArray.of(items);

            return { ...s, contributors, maxContributors, isLoaded: true, version, canCreate };
        });
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }
}