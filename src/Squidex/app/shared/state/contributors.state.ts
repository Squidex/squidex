/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, distinctUntilChanged, map, tap } from 'rxjs/operators';

import {
    DialogService,
    ErrorDto,
    ImmutableArray,
    ResourceLinks,
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

    // Indicates if the maximum number of contributors are reached.
    isMaxReached?: boolean;

    // Indicates if the contributors are loaded.
    isLoaded?: boolean;

    // The maximum allowed users.
    maxContributors: number;

    // The app version.
    version: Version;

    // The links.
    links: ResourceLinks;
}

type ContributorsList = ImmutableArray<ContributorDto>;

@Injectable()
export class ContributorsState extends State<Snapshot> {
    public contributors =
        this.changes.pipe(map(x => x.contributors),
            distinctUntilChanged());

    public isMaxReached =
        this.changes.pipe(map(x => x.isMaxReached),
            distinctUntilChanged());

    public isLoaded =
        this.changes.pipe(map(x => !!x.isLoaded),
            distinctUntilChanged());

    public maxContributors =
        this.changes.pipe(map(x => x.maxContributors),
            distinctUntilChanged());

    public links =
        this.changes.pipe(map(x => x.links),
            distinctUntilChanged());

    constructor(
        private readonly contributorsService: ContributorsService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService
    ) {
        super({ contributors: ImmutableArray.empty(), version: new Version(''), maxContributors: -1, links: {} });
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
            const maxContributors = payload.maxContributors || s.maxContributors;

            const isLoaded = true;
            const isMaxReached = maxContributors > 0 && maxContributors <= payload.items.length;

            const contributors = ImmutableArray.of(payload.items);

            return { ...s, contributors, maxContributors, isLoaded, isMaxReached, version: version, links: payload._links };
        });
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }
}