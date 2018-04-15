/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Observable } from 'rxjs';

import {
    AppContributorDto,
    AppsState,
    AssignContributorForm,
    AutocompleteSource,
    ContributorsState,
    PublicUserDto,
    UsersService
} from '@app/shared';

export class UsersDataSource implements AutocompleteSource {
    constructor(
        private readonly usersService: UsersService,
        private readonly component: ContributorsPageComponent
    ) {
    }

    public find(query: string): Observable<any[]> {
        return this.usersService.getUsers(query)
            .withLatestFrom(this.component.contributorsState.contributors, (users, contributors) => {
                const results: any[] = [];

                for (let user of users) {
                    if (!contributors.find(t => t.contributor.contributorId === user.id)) {
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
export class ContributorsPageComponent implements OnInit {
    public usersDataSource: UsersDataSource;
    public usersPermissions = [ 'Owner', 'Developer', 'Editor' ];

    public assignContributorForm = new AssignContributorForm(this.formBuilder);

    constructor(
        public readonly appsState: AppsState,
        public readonly contributorsState: ContributorsState,
        private readonly formBuilder: FormBuilder,
        usersService: UsersService
    ) {
        this.usersDataSource = new UsersDataSource(usersService, this);
    }

    public ngOnInit() {
        this.contributorsState.load().onErrorResumeNext().subscribe();
    }

    public removeContributor(contributor: AppContributorDto) {
        this.contributorsState.revoke(contributor).onErrorResumeNext().subscribe();
    }

    public changePermission(contributor: AppContributorDto, permission: string) {
        this.contributorsState.assign(new AppContributorDto(contributor.contributorId, permission)).onErrorResumeNext().subscribe();
    }

    public assignContributor() {
        const value = this.assignContributorForm.submit();

        if (value) {
            let user = value.user;

            if (user instanceof PublicUserDto) {
                user = user.id;
            }

            const requestDto = new AppContributorDto(user, 'Editor');

            this.contributorsState.assign(requestDto)
                .subscribe(dto => {
                    this.assignContributorForm.submitCompleted();
                }, error => {
                    this.assignContributorForm.submitFailed(error);
                });
        }
    }
}
