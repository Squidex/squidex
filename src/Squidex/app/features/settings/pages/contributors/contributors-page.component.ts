/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

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

class UsersDataSource implements AutocompleteSource {
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

@Ng2.Component({
    selector: 'sqx-contributor-page',
    styles,
    template
})
export class ContributorsPageComponent extends AppComponentBase implements Ng2.OnInit {
    public appContributors = ImmutableArray.empty<AppContributorDto>();

    public currentUserId: string;

    public selectedUserName: string | null = null;
    public selectedUser: UserDto | null = null;

    public usersDataSource: UsersDataSource;
    public usersPermissions = [
        'Owner',
        'Developer',
        'Editor'
    ];

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly appContributorsService: AppContributorsService,
        private readonly usersService: UsersService,
        private readonly authService: AuthService
    ) {
        super(apps, notifications, users);

        this.usersDataSource = new UsersDataSource(usersService, this);
    }

    public ngOnInit() {
        this.currentUserId = this.authService.user.id;

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
        if (!this.selectedUser) {
            return;
        }

        const contributor = new AppContributorDto(this.selectedUser.id, 'Editor');

        this.selectedUser = null;
        this.selectedUserName = null;

        this.appName()
            .switchMap(app => this.appContributorsService.postContributor(app, contributor))
            .subscribe(() => {
                this.appContributors = this.appContributors.push(contributor);
            }, error => {
                this.notifyError(error);
            });
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

    public selectUser(selection: UserDto) {
        this.selectedUser = selection;
    }
}

