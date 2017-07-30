/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Observable } from 'rxjs';

import {
    AppComponentBase,
    AppContributorDto,
    AppContributorsDto,
    AppContributorsService,
    AppsStoreService,
    AuthService,
    AutocompleteSource,
    HistoryChannelUpdated,
    ImmutableArray,
    MessageBus,
    NotificationService,
    UsersService,
    Version
} from 'shared';

export class UsersDataSource implements AutocompleteSource {
    constructor(
        private readonly usersService: UsersService,
        private readonly component: ContributorsPageComponent
    ) {
    }

    public find(query: string): Observable<any[]> {
        return this.usersService.getUsers(query)
            .map(users => {
                const results: any[] = [];

                for (let user of users) {
                    if (!this.component.appContributors || !this.component.appContributors.find(t => t.contributorId === user.id)) {
                        results.push(user);
                    }
                }
                return results;
            });
    }
}

@Component({
    selector: 'sqx-contributors-page',
    styleUrls: ['./contributors-page.component.scss'],
    templateUrl: './contributors-page.component.html'
})
export class ContributorsPageComponent extends AppComponentBase implements OnInit {
    private version = new Version();

    public appContributors = ImmutableArray.empty<AppContributorDto>();

    public currentUserId: string;

    public maxContributors = -1;

    public usersDataSource: UsersDataSource;
    public usersPermissions = [
        'Owner',
        'Developer',
        'Editor',
        'Reader'
    ];

    public get canAddContributor() {
        return this.addContributorForm.valid && (this.maxContributors <= -1 || this.appContributors.length < this.maxContributors);
    }

    public addContributorForm =
        this.formBuilder.group({
            user: [null,
                [
                    Validators.required
                ]]
        });

    constructor(apps: AppsStoreService, notifications: NotificationService, usersService: UsersService,
        private readonly appContributorsService: AppContributorsService,
        private readonly messageBus: MessageBus,
        private readonly authService: AuthService,
        private readonly formBuilder: FormBuilder
    ) {
        super(notifications, apps);

        this.usersDataSource = new UsersDataSource(usersService, this);
    }

    public ngOnInit() {
        this.currentUserId = this.authService.user!.id;

        this.load();
    }

    public load() {
        this.appNameOnce()
            .switchMap(app => this.appContributorsService.getContributors(app, this.version).retry(2))
            .subscribe(dto => {
                this.updateContributorsFromDto(dto);
            }, error => {
                this.notifyError(error);
            });
    }

    public removeContributor(contributor: AppContributorDto) {
        this.appNameOnce()
            .switchMap(app => this.appContributorsService.deleteContributor(app, contributor.contributorId, this.version))
            .subscribe(() => {
                this.updateContributors(this.appContributors.remove(contributor));
            }, error => {
                this.notifyError(error);
            });
    }

    public changePermission(contributor: AppContributorDto, permission: string) {
        const requestDto = contributor.changePermission(permission);

        this.appNameOnce()
            .switchMap(app => this.appContributorsService.postContributor(app, requestDto, this.version))
            .subscribe(() => {
                this.updateContributors(this.appContributors.replace(contributor, requestDto));
            }, error => {
                this.notifyError(error);
            });
    }

    public assignContributor() {
        const requestDto = new AppContributorDto(this.addContributorForm.controls['user'].value.id, 'Editor');

        this.appNameOnce()
            .switchMap(app => this.appContributorsService.postContributor(app, requestDto, this.version))
            .subscribe(() => {
                this.updateContributors(this.appContributors.push(requestDto));
                this.resetContributorForm();
            }, error => {
                this.notifyError(error);
                this.resetContributorForm();
            });
    }

    private resetContributorForm() {
        this.addContributorForm.reset();
    }

    private updateContributorsFromDto(dto: AppContributorsDto) {
        this.updateContributors(ImmutableArray.of(dto.contributors));

        this.maxContributors = dto.maxContributors;
    }

    private updateContributors(contributors: ImmutableArray<AppContributorDto>) {
        this.appContributors = contributors;

        this.messageBus.emit(new HistoryChannelUpdated());
    }
}
