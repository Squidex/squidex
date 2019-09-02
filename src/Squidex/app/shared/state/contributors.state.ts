/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { combineLatest, Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

import {
    DialogService,
    ErrorDto,
    ImmutableArray,
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

    // Indicates if the contributors are loaded.
    isLoaded?: boolean;

    // The maximum allowed users.
    maxContributors: number;

    // The current page.
    page: number;

    // The search query.
    query?: string;

    // Query regex.
    queryRegex?: RegExp;

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

    public page =
        this.project(x => x.page);

    public query =
        this.project(x => x.query);

    public queryRegex =
        this.project(x => x.queryRegex);

    public maxContributors =
        this.project(x => x.maxContributors);

    public isLoaded =
        this.project(x => !!x.isLoaded);

    public canCreate =
        this.project(x => !!x.canCreate);

    public filtered =
        combineLatest(this.queryRegex, this.contributors, (q, c) => getFilteredContributors(c, q));

    public contributorsPaged =
        combineLatest(this.page, this.filtered, (p, c) => getPagedContributors(c, p));

    public contributorsPager =
        combineLatest(this.page, this.filtered, (p, c) => new Pager(c.length, p, PAGE_SIZE));

    constructor(
        private readonly contributorsService: ContributorsService,
        private readonly appsState: AppsState,
        private readonly dialogs: DialogService
    ) {
        super({ contributors: ImmutableArray.empty(), page: 0, maxContributors: -1, version: Version.EMPTY });
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

    public goNext() {
        this.next(s => ({ ...s, page: s.page + 1 }));
    }

    public goPrev() {
        this.next(s => ({ ...s, page: s.page - 1 }));
    }

    public search(query: string) {
        this.next(s => ({ ...s, query, queryRegex: new RegExp(query) }));
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

const PAGE_SIZE = 10;

function getPagedContributors(contributors: ContributorsList, page: number) {
    return ImmutableArray.of(contributors.values.slice(page * PAGE_SIZE, (page + 1) * PAGE_SIZE));
}

function getFilteredContributors(contributors: ContributorsList, query?: RegExp) {
    let filtered = contributors;

    if (query) {
        filtered = filtered.filter(x => query.test(x.contributorName));
    }

    return filtered;
}
