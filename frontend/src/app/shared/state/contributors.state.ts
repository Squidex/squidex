/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { EMPTY, Observable, throwError } from 'rxjs';
import { catchError, finalize, tap } from 'rxjs/operators';
import { DialogService, ErrorDto, getPagingInfo, ListState, shareMapSubscribed, shareSubscribed, State, Types, Version } from '@app/framework';
import { AssignContributorDto, ContributorDto, ContributorsPayload, ContributorsService } from './../services/contributors.service';
import { AppsState } from './apps.state';

interface Snapshot extends ListState<string> {
    // The current contributors.
    contributors: ReadonlyArray<ContributorDto>;

    // The maximum allowed users.
    maxContributors: number;

    // The app version.
    version: Version;

    // Indicates if the user can add a contributor.
    canCreate?: boolean;
}

@Injectable()
export class ContributorsState extends State<Snapshot> {
    public contributors =
        this.project(x => x.contributors);

    public paging =
        this.project(x => getPagingInfo(x, x.contributors.length));

    public query =
        this.project(x => x.query);

    public queryRegex =
        this.projectFrom(this.query, x => getRegex(x));

    public maxContributors =
        this.project(x => x.maxContributors);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    public contributorsFiltered =
        this.project(getFilteredContributors);

    public get appId() {
        return this.appsState.appId;
    }

    public get appName() {
        return this.appsState.appName;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly contributorsService: ContributorsService,
        private readonly dialogs: DialogService,
    ) {
        super({
            contributors: [],
            maxContributors: -1,
            page: 0,
            pageSize: 10,
            total: 0,
            version: Version.EMPTY,
        }, 'Contributors');
    }

    public loadIfNotLoaded(): Observable<any> {
        if (this.snapshot.isLoaded) {
            return EMPTY;
        }

        return this.loadInternal(false);
    }

    public load(isReload = false, update: Partial<Snapshot> = {}): Observable<any> {
        if (!isReload) {
            this.resetState(update, 'Loading Initial');
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        this.next({ isLoading: true }, 'Loading Started');

        return this.contributorsService.getContributors(this.appName).pipe(
            tap(({ version, payload }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:contributors.reloaded');
                }

                this.replaceContributors(version, payload);
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs));
    }

    public page(paging: { page: number; pageSize: number }) {
        this.next(paging, 'Results Paged');
    }

    public search(query: string) {
        this.next({ query }, 'Results Filtered');
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
                    return throwError(() => new ErrorDto(404, 'i18n:contributors.userNotFound'));
                } else {
                    return throwError(() => error);
                }
            }),
            tap(({ version, payload }) => {
                this.replaceContributors(version, payload);
            }),
            shareMapSubscribed(this.dialogs, x => x.payload.isInvited, options));
    }

    private replaceContributors(version: Version, { canCreate, items, maxContributors }: ContributorsPayload) {
        this.next({
            canCreate,
            contributors: items,
            isLoaded: true,
            isLoading: false,
            maxContributors,
            total: items.length,
            version,
        }, 'Loading Success / Updated');
    }

    private get version() {
        return this.snapshot.version;
    }
}

function getRegex(query?: string): RegExp | undefined {
    return query ? new RegExp(query, 'i') : undefined;
}

function getFilteredContributors(snapshot: Snapshot) {
    const { contributors, query, page, pageSize } = snapshot;

    let filtered = contributors;

    if (query) {
        const regex = new RegExp(query, 'i');

        filtered = filtered.filter(x => regex.test(x.contributorName));
    }

    return filtered.slice(page * pageSize, (page + 1) * pageSize);
}
