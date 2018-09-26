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
    notify,
    State,
    Types,
    Version
} from '@app/framework';

import { AppContributorDto, AppContributorsService } from './../services/app-contributors.service';
import { AuthService } from './../services/auth.service';
import { AppsState } from './apps.state';

interface SnapshotContributor {
    contributor: AppContributorDto;

    isCurrentUser: boolean;
}

interface Snapshot {
    contributors: ImmutableArray<SnapshotContributor>;

    isMaxReached?: boolean;
    isLoaded?: boolean;

    maxContributors: number;

    version: Version;
}

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
        private readonly appContributorsService: AppContributorsService,
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

        return this.appContributorsService.getContributors(this.appName).pipe(
            tap(dtos => {
                if (isReload) {
                    this.dialogs.notifyInfo('Contributors reloaded.');
                }

                const contributors = ImmutableArray.of(dtos.contributors.map(x => this.createContributor(x)));

                this.replaceContributors(contributors, dtos.version, dtos.maxContributors);
            }),
            notify(this.dialogs));
    }

    public revoke(contributor: AppContributorDto): Observable<any> {
        return this.appContributorsService.deleteContributor(this.appName, contributor.contributorId, this.version).pipe(
            tap(dto => {
                const contributors = this.snapshot.contributors.filter(x => x.contributor.contributorId !== contributor.contributorId);

                this.replaceContributors(contributors, dto.version);
            }),
            notify(this.dialogs));
    }

    public assign(request: AppContributorDto): Observable<any> {
        return this.appContributorsService.postContributor(this.appName, request, this.version).pipe(
            tap(dto => {
                const contributors = this.updateContributors(dto.payload.contributorId, request.permission, dto.version);

                this.replaceContributors(contributors, dto.version);
            }),
            catchError(error => {
                if (Types.is(error, ErrorDto) && error.statusCode === 404) {
                    return throwError(new ErrorDto(404, 'The user does not exist.'));
                } else {
                    return throwError(error);
                }
            }),
            notify(this.dialogs));
    }

    private updateContributors(id: string, permission: string, version: Version) {
        const contributor = new AppContributorDto(id, permission);
        const contributors = this.snapshot.contributors;

        if (contributors.find(x => x.contributor.contributorId === id)) {
            return contributors.map(x => x.contributor.contributorId === id ? this.createContributor(contributor, x) : x);
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

    private createContributor(contributor: AppContributorDto, current?: SnapshotContributor): SnapshotContributor {
        if (!contributor) {
            return null!;
        } else if (current && current.contributor === contributor) {
            return current;
        } else {
            return { contributor, isCurrentUser: contributor.contributorId === this.userId };
        }
    }
}