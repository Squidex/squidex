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
    shareSubscribed,
    State,
    Types,
    Version
} from '@app/framework';

import {
    AssignContributorDto,
    ContributorDto,
    ContributorsService
} from './../services/contributors.service';

import { AuthService } from './../services/auth.service';
import { AppsState } from './apps.state';

interface SnapshotContributor {
    // The contributor.
    contributor: ContributorDto;

    // Indicates if the contributor is the ucrrent user.
    isCurrentUser: boolean;
}

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
}

type ContributorsList = ImmutableArray<SnapshotContributor>;

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

    constructor(
        private readonly contributorsService: ContributorsService,
        private readonly appsState: AppsState,
        private readonly authState: AuthService,
        private readonly dialogs: DialogService
    ) {
        super({ contributors: ImmutableArray.empty(), version: new Version(''), maxContributors: -1 });
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

                const contributors = ImmutableArray.of(payload.contributors.map(x => this.createContributor(x)));

                this.replaceContributors(contributors, version, payload.maxContributors);
            }),
            shareSubscribed(this.dialogs));
    }

    public revoke(contributor: ContributorDto): Observable<any> {
        return this.contributorsService.deleteContributor(this.appName, contributor.contributorId, this.version).pipe(
            tap(({ version }) => {
                const contributors = this.snapshot.contributors.filter(x => x.contributor.contributorId !== contributor.contributorId);

                this.replaceContributors(contributors, version);
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
                const contributors = this.updateContributors(payload.contributorId, request.role);

                this.replaceContributors(contributors, version);
            }),
            shareSubscribed(this.dialogs, { project: x => x.payload.isCreated }));
    }

    private updateContributors(id: string, role: string) {
        const contributor = new ContributorDto(id, role);
        const contributors = this.snapshot.contributors;

        if (contributors.find(x => x.contributor.contributorId === id)) {
            return contributors.map(x => x.contributor.contributorId === id ? this.createContributor(contributor) : x);
        } else {
            return contributors.push(this.createContributor(contributor));
        }
    }

    private replaceContributors(contributors: ImmutableArray<SnapshotContributor>, version: Version, maxContributors?: number) {
        this.next(s => {
            maxContributors = maxContributors || s.maxContributors;

            const isLoaded = true;
            const isMaxReached = maxContributors > 0 && maxContributors <= contributors.length;

            return { ...s, contributors, maxContributors, isLoaded, isMaxReached, version };
        });
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get userId() {
        return this.authState.user!.id;
    }

    private get version() {
        return this.snapshot.version;
    }

    private createContributor(contributor: ContributorDto): SnapshotContributor {
        return { contributor, isCurrentUser: contributor.contributorId === this.userId };
    }
}