/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

import '@app/framework/utils/rxjs-extensions';

import {
    DialogService,
    ImmutableArray,
    Form,
    State,
    Version
} from '@app/framework';

import { AuthService } from './../services/auth.service';
import { AppsState } from './apps.state';
import { AppContributorDto, AppContributorsService } from './../services/app-contributors.service';

export class AssignContributorForm extends Form<FormGroup> {
    public hasNoUser =
        this.form.controls['user'].valueChanges.startWith(null).map(x => !x);

    constructor(formBuilder: FormBuilder) {
        super(formBuilder.group({
            user: [null,
                [
                    Validators.required
                ]
            ]
        }));
    }
}

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
        this.changes.map(x => x.contributors)
            .distinctUntilChanged();

    public isMaxReached =
        this.changes.map(x => x.isMaxReached)
            .distinctUntilChanged();

    public isLoaded =
        this.changes.map(x => !!x.isLoaded)
            .distinctUntilChanged();

    public maxContributors =
        this.changes.map(x => x.maxContributors)
            .distinctUntilChanged();

    constructor(
        private readonly appContributorsService: AppContributorsService,
        private readonly appsState: AppsState,
        private readonly authState: AuthService,
        private readonly dialogs: DialogService
    ) {
        super({ contributors: ImmutableArray.empty(), version: new Version(''), maxContributors: -1 });
    }

    public load(notifyLoad = false): Observable<any> {
        return this.appContributorsService.getContributors(this.appName)
            .do(dtos => {
                if (notifyLoad) {
                    this.dialogs.notifyInfo('Contributors reloaded.');
                }

                const contributors = ImmutableArray.of(dtos.contributors.map(x => this.createContributor(x)));

                this.replaceContributors(contributors, dtos.version, dtos.maxContributors);
            })
            .notify(this.dialogs);
    }

    public revoke(contributor: AppContributorDto): Observable<any> {
        return this.appContributorsService.deleteContributor(this.appName, contributor.contributorId, this.version)
            .do(dto => {
                const contributors = this.snapshot.contributors.filter(x => x.contributor.contributorId !== contributor.contributorId);

                this.replaceContributors(contributors, dto.version);
            })
            .notify(this.dialogs);
    }

    public assign(request: AppContributorDto): Observable<any> {
        return this.appContributorsService.postContributor(this.appName, request, this.version)
            .do(dto => {
                const contributor = this.createContributor(new AppContributorDto(dto.payload.contributorId, request.permission));

                let contributors = this.snapshot.contributors;

                if (contributors.find(x => x.contributor.contributorId === dto.payload.contributorId)) {
                    contributors = contributors.map(c => c.contributor.contributorId === dto.payload.contributorId ? contributor : c);
                } else {
                    contributors = contributors.push(contributor);
                }

                this.replaceContributors(contributors, dto.version);
            })
            .notify(this.dialogs);
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

    private get version() {
        return this.snapshot.version;
    }

    private createContributor(contributor: AppContributorDto): SnapshotContributor {
        return { contributor, isCurrentUser: contributor.contributorId === this.authState.user!.id };
    }
}