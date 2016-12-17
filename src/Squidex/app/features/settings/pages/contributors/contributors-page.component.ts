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
    ImmutableArray,
    NotificationService,
    UserDto,
    UsersProviderService,
    UsersService
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
                                user.pictureUrl,
                                user));
                    }
                }
                return results;
            });
    }
}

function changePermission(contributor: AppContributorDto, permission: string): AppContributorDto {
    return new AppContributorDto(contributor.contributorId, permission);
}

@Component({
    selector: 'sqx-contributors-page',
    styleUrls: ['./contributors-page.component.scss'],
    templateUrl: './contributors-page.component.html'
})
export class ContributorsPageComponent extends AppComponentBase implements OnInit {
    public appContributors = ImmutableArray.empty<AppContributorDto>();

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

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly appContributorsService: AppContributorsService,
        private readonly usersService: UsersService,
        private readonly authService: AuthService,
        private readonly formBuilder: FormBuilder
    ) {
        super(apps, notifications, users);

        this.usersDataSource = new UsersDataSource(usersService, this);
    }

    public ngOnInit() {
        this.load();
    }

    public load() {
        this.appName()
            .switchMap(app => this.appContributorsService.getContributors(app).retry(2))
            .subscribe(dtos => {
                this.appContributors = ImmutableArray.of(dtos);
            }, error => {
                this.notifyError(error);
            });
    }

    public assignContributor() {
        const contributor = new AppContributorDto(this.addContributorForm.get('user').value.model.id, 'Editor');

        this.appName()
            .switchMap(app => this.appContributorsService.postContributor(app, contributor))
            .subscribe(() => {
                this.appContributors = this.appContributors.push(contributor);
            }, error => {
                this.notifyError(error);
            });

        this.addContributorForm.reset();
    }

    public changePermission(contributor: AppContributorDto, permission: string) {
        const newContributor = changePermission(contributor, permission);

        this.appName()
            .switchMap(app => this.appContributorsService.postContributor(app, newContributor))
            .subscribe(() => {
                this.appContributors = this.appContributors.replace(contributor, newContributor);
            }, error => {
                this.notifyError(error);
            });
    }

    public removeContributor(contributor: AppContributorDto) {
        this.appName()
            .switchMap(app => this.appContributorsService.deleteContributor(app, contributor.contributorId))
            .subscribe(() => {
                this.appContributors = this.appContributors.remove(contributor);
            }, error => {
                this.notifyError(error);
            });
    }
}

