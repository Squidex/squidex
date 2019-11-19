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
    Pager,
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

    // The pagination information.
    contributorsPager: Pager;

    // Indicates if the contributors are loaded.
    isLoaded?: boolean;

    // The maximum allowed users.
    maxContributors: number;

    // The search query.
    query?: string;

    // Query regex.
    queryRegex?: RegExp;

    // The app version.
    version: Version;

    // Indicates if the user can add a contributor.
    canCreate?: boolean;
}

type ContributorsList = ReadonlyArray<ContributorDto>;

@Injectable()
export class ContributorsState extends State<Snapshot> {
    public contributors =
        this.project(x => x.contributors);

    public query =
        this.project(x => x.query);

    public queryRegex =
        this.project(x => x.queryRegex);

    public maxContributors =
        this.project(x => x.maxContributors);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    public filtered =
        this.projectFrom2(this.queryRegex, this.contributors, (q, c) => getFilteredContributors(c, q));

    public contributorsPager =
        this.project(x => x.contributorsPager);

    public contributorsPaged =
        this.projectFrom2(this.contributorsPager, this.filtered, (p, c) => getPagedContributors(c, p));

    constructor(
        private readonly contributorsService: ContributorsService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService
    ) {
        super({ contributors: [], contributorsPager: Pager.DEFAULT, maxContributors: -1, version: Version.EMPTY });
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

    public setPager(contentsPager: Pager) {
        this.next(s => ({ ...s, contentsPager }));
    }

    public search(query: string) {
        this.next(s => ({ ...s, query, queryRegex: new RegExp(query, 'i') }));
    }

    public revoke(contributor: ContributorDto): Observable<any> {
        return this.contributorsService.deleteContributor(this.appName, contributor, this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceContributors(version, payload);
            }),
            shareSubscribed(this.dialogs));
    }

    public assign(request: AssignContributorDto, options?: { silent: boolean }): Observable<boolean | undefined> {
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
            shareMapSubscribed(this.dialogs, x => x.payload._meta && x.payload._meta['isInvited'] === '1', options));
    }

    private replaceContributors(version: Version, payload: ContributorsPayload) {
        this.next(() => {
            const { canCreate, items: contributors, maxContributors } = payload;

            return {
                canCreate,
                contributors,
                isLoaded: true,
                maxContributors,
                page: 0,
                version
            };
        });
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get version() {
        return this.snapshot.version;
    }
}

function getPagedContributors(contributors: ContributorsList, pager: Pager) {
    return contributors.slice(pager.page * pager.pageSize, (pager.page + 1) * pager.pageSize);
}

function getFilteredContributors(contributors: ContributorsList, query?: RegExp) {
    let filtered = contributors;

    if (query) {
        filtered = filtered.filter(x => query.test(x.contributorName));
    }

    return filtered;
}
