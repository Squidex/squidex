/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { DialogService, ErrorDto, Pager, shareMapSubscribed, shareSubscribed, State, StateSynchronizer, Types, Version } from '@app/framework';
import { EMPTY, Observable, throwError } from 'rxjs';
import { catchError, finalize, tap } from 'rxjs/operators';
import { AssignContributorDto, ContributorDto, ContributorsPayload, ContributorsService } from './../services/contributors.service';
import { AppsState } from './apps.state';

interface Snapshot {
    // All loaded contributors.
    contributors: ContributorsList;

    // The pagination information.
    contributorsPager: Pager;

    // Indicates if the contributors are loaded.
    isLoaded?: boolean;

    // Indicates if the contributors are loading.
    isLoading?: boolean;

    // The maximum allowed users.
    maxContributors: number;

    // The search query.
    query?: string;

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
        this.projectFrom(this.query, q => q ? new RegExp(q, 'i') : undefined);

    public maxContributors =
        this.project(x => x.maxContributors);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    public filtered =
        this.projectFrom2(this.queryRegex, this.contributors, (q, c) => getFilteredContributors(c, q));

    public contributorsPager =
        this.project(x => x.contributorsPager);

    public contributorsPaged =
        this.projectFrom2(this.contributorsPager, this.filtered, (p, c) => getPagedContributors(c, p));

    public get appId() {
        return this.appsState.appId;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly contributorsService: ContributorsService,
        private readonly dialogs: DialogService
    ) {
        super({
            contributors: [],
            contributorsPager: new Pager(0),
            maxContributors: -1,
            version: Version.EMPTY
        });
    }

    public loadAndListen(synchronizer: StateSynchronizer) {
        synchronizer.mapTo(this)
            .withString('query', 'q')
            .withPager('contributorsPager', 'contributors', 10)
            .whenSynced(() => this.loadInternal(false))
            .build();
    }

    public loadIfNotLoaded(): Observable<any> {
        if (this.snapshot.isLoaded) {
            return EMPTY;
        }

        return this.loadInternal(false);
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            const contributorsPager = this.snapshot.contributorsPager.reset();

            this.resetState({ contributorsPager });
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        this.next({ isLoading: true });

        return this.contributorsService.getContributors(this.appName).pipe(
            tap(({ version, payload }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:contributors.reloaded');
                }

                this.replaceContributors(version, payload);
            }),
            finalize(() => {
                this.next({ isLoading: false });
            }),
            shareSubscribed(this.dialogs));
    }

    public setPager(contributorsPager: Pager) {
        this.next(s => ({ ...s, contributorsPager }));
    }

    public search(query: string) {
        this.next(s => ({ ...s, query }));
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
                    return throwError(new ErrorDto(404, 'i18n:contributors.userNotFound'));
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
        this.next(s => {
            const { canCreate, items: contributors, maxContributors } = payload;

            const contributorsPager = s.contributorsPager.setCount(contributors.length);

            return {
                ...s,
                canCreate,
                contributors,
                contributorsPager,
                isLoaded: true,
                isLoading: false,
                maxContributors,
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
