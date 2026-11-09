/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed, inject, Service } from '@angular/core';
import { EMPTY, Observable, throwError } from 'rxjs';
import { catchError, finalize, tap } from 'rxjs/operators';
import { AssignContributorDto, ContributorDto, ContributorsDto, debug, DialogService, ErrorDto, getPagingInfo, ListState, shareMapSubscribed, shareSubscribed, State, StringHelper, TeamsState, Types, VersionTag } from '@app/shared';
import { TeamContributorsService } from '../internal';

interface Snapshot extends ListState<string> {
    // The current contributors.
    contributors: ReadonlyArray<ContributorDto>;

    // The maximum allowed users.
    maxContributors: number;

    // The team version.
    version: VersionTag;

    // Indicates if the user can add a contributor.
    canCreate?: boolean;
}

@Service({ autoProvided: false })
export class TeamContributorsState extends State<Snapshot> {
    private readonly contributorsService = inject(TeamContributorsService);
    private readonly dialogs = inject(DialogService);
    private readonly teamsState = inject(TeamsState);

    public readonly contributors =
        this.project(x => x.contributors);

    public readonly paging =
        this.project(x => getPagingInfo(x, x.contributors.length));

    public readonly query =
        this.project(x => x.query);

    public readonly queryRegex =
        computed(() => getRegex(this.query()));

    public readonly maxContributors =
        this.project(x => x.maxContributors);

    public readonly isLoaded =
        this.project(x => x.isLoaded === true);

    public readonly isLoading =
        this.project(x => x.isLoading === true);

    public readonly canCreate =
        this.project(x => x.canCreate === true);

    public readonly contributorsFiltered =
        this.project(getFilteredContributors);

    public get teamId() {
        return this.teamsState.teamId;
    }

    constructor() {
        super({
            contributors: [],
            maxContributors: -1,
            page: 0,
            pageSize: 10,
            total: 0,
            version: VersionTag.EMPTY,
        });

        debug(this, 'teamContributors');
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

        return this.contributorsService.getContributors(this.teamId).pipe(
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
        return this.contributorsService.deleteContributor(this.teamId, contributor, this.version).pipe(
            tap(({ version, payload }) => {
                this.replaceContributors(version, payload);
            }),
            shareSubscribed(this.dialogs));
    }

    public assign(request: AssignContributorDto, options?: { silent: boolean }): Observable<boolean | undefined> {
        return this.contributorsService.postContributor(this.teamId, request, this.version).pipe(
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

    private replaceContributors(version: VersionTag, payload: ContributorsDto) {
        const { canCreate, items, maxContributors } = payload;

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
    return query ? StringHelper.searchRegex(query) : undefined;
}

function getFilteredContributors(snapshot: Snapshot) {
    const { contributors, query, page, pageSize } = snapshot;

    let filtered = contributors;

    if (query) {
        const regex = StringHelper.searchRegex(query);

        filtered = filtered.filter(x => regex.test(x.contributorName));
    }

    return filtered.slice(page * pageSize, (page + 1) * pageSize);
}
