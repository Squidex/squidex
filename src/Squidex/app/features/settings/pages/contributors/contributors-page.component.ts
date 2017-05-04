/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Observable } from 'rxjs';

import {
    AppComponentBase,
    AppContributorDto,
    AppContributorsService,
    AppsStoreService,
    AuthService,
    AutocompleteItem,
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

    public find(query: string): Observable<AutocompleteItem[]> {
        return this.usersService.getUsers(query)
            .map(users => {
                const results: AutocompleteItem[] = [];

                for (let user of users) {
                    if (!this.component.appContributors || !this.component.appContributors.find(t => t.contributorId === user.id)) {
                        results.push(
                            new AutocompleteItem(
                                user.displayName,
                                user.email,
                                user.pictureUrl!,
                                user));
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

    public usersDataSource: UsersDataSource;
    public usersPermissions = [
        'Owner',
        'Developer',
        'Editor'
    ];

    public addContributorForm: FormGroup =
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
            .subscribe(dtos => {
                this.updateContributors(ImmutableArray.of(dtos));
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

    public assignContributor() {
        const newContributor = new AppContributorDto(this.addContributorForm.get('user')!.value.model.id, 'Editor');

        this.appNameOnce()
            .switchMap(app => this.appContributorsService.postContributor(app, newContributor, this.version))
            .subscribe(() => {
                this.updateContributors(this.appContributors.push(newContributor));
            }, error => {
                this.notifyError(error);
            });

        this.addContributorForm.reset();
    }

    public changePermission(contributor: AppContributorDto, permission: string) {
        const newContributor = changePermission(contributor, permission);

        this.appNameOnce()
            .switchMap(app => this.appContributorsService.postContributor(app, newContributor, this.version))
            .subscribe(() => {
                this.updateContributors(this.appContributors.replace(contributor, newContributor));
            }, error => {
                this.notifyError(error);
            });
    }

    private updateContributors(contributors: ImmutableArray<AppContributorDto>) {
        this.appContributors = contributors;

        this.messageBus.publish(new HistoryChannelUpdated());
    }
}

function changePermission(contributor: AppContributorDto, permission: string): AppContributorDto {
    return new AppContributorDto(contributor.contributorId, permission);
}

