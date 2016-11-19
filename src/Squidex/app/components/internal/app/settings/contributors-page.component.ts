/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { Observable, Subscription } from 'rxjs';

import { CompleterBaseData, CompleterItem } from 'ng2-completer';

import {
    AppContributorDto,
    AppContributorsService,
    AppsStoreService,
    AuthService,
    TitleService,
    UserDto,
    UsersService,
    UsersProviderService
} from 'shared';

class UsersDataSource extends CompleterBaseData {
    private remoteSearch: Subscription;

    constructor(
        private readonly usersService: UsersService,
        private readonly component: ContributorsPageComponent,
    ) {
        super();
    }

    public search(term: string): void {
        this.cancel();

        this.remoteSearch = 
            this.usersService.getUsers(term)
                .map(users => {
                    const results: CompleterItem[] = [];

                    for (let u of users) {
                        if (!this.component.appContributors || !this.component.appContributors.find(t => t.contributorId === u.id)) {
                            results.push({ title: u.displayName, image: u.pictureUrl, originalObject: u, description: u.email });
                        }
                    }

                    this.next(results);

                    return results;
                })
                .catch(err => {
                    this.error(err);

                    return null;
                }).subscribe();
    }

    public cancel() {
        if (this.remoteSearch) {
            this.remoteSearch.unsubscribe();
        }
    }
}

@Ng2.Component({
    selector: 'sqx-contributor-page',
    styles,
    template
})
export class ContributorsPageComponent implements Ng2.OnInit {
    private appSubscription: any | null = null;
    private appName: string;

    public appContributors: AppContributorDto[];

    public selectedUserName: string | null = null;
    public selectedUser: UserDto | null = null;

    public currrentUserId: string;

    public usersDataSource: UsersDataSource;
    public usersPermissions = [
        'Owner',
        'Developer',
        'Editor'
    ];

    constructor(
        private readonly titles: TitleService,
        private readonly authService: AuthService,
        private readonly appsStore: AppsStoreService,
        private readonly appContributorsService: AppContributorsService,
        private readonly usersProvider: UsersProviderService,
        private readonly usersService: UsersService
    ) {
        this.usersDataSource = new UsersDataSource(usersService, this);
    }

    public ngOnInit() {
        this.currrentUserId = this.authService.user.id;

        this.appSubscription =
            this.appsStore.selectedApp.subscribe(app => {
                if (app) {
                    this.appName = app.name;

                    this.titles.setTitle('{appName} | Settings | Contributors', { appName: app.name });

                    this.appContributorsService.getContributors(app.name).subscribe(contributors => {
                        this.appContributors = contributors;
                    });
                }
            });
    }

    public ngOnDestroy() {
        this.appSubscription.unsubscribe();
    }

    public addContributor() {
        if (!this.selectedUser) {
            return;
        }

        const contributor = new AppContributorDto(this.selectedUser.id, 'Editor');

        this.appContributorsService.postContributor(this.appName, contributor).subscribe();
        this.appContributors.push(contributor);

        this.selectedUser = null;
        this.selectedUserName = null;
    }

    public removeContributor(contributor: AppContributorDto) {
        this.appContributorsService.deleteContributor(this.appName, contributor.contributorId).subscribe();

        this.appContributors.splice(this.appContributors.indexOf(contributor), 1);
    }

    public saveContributor(contributor: AppContributorDto) {
        this.appContributorsService.postContributor(this.appName, contributor).subscribe();
    }

    public selectUser(selection: CompleterItem | null) {
        this.selectedUser = selection ? selection.originalObject : null;
    }

    public email(contributor: AppContributorDto): Observable<string> {
        return this.usersProvider.getUser(contributor.contributorId).map(u => u.email);
    }

    public displayName(contributor: AppContributorDto): Observable<string> {
        return this.usersProvider.getUser(contributor.contributorId).map(u => u.displayName);
    }

    public pictureUrl(contributor: AppContributorDto): Observable<string> {
        return this.usersProvider.getUser(contributor.contributorId).map(u => u.pictureUrl);
    }
}

