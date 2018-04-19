/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Injectable, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Observable } from 'rxjs';

import {
    AppContributorDto,
    AppsState,
    AssignContributorForm,
    AutocompleteSource,
    ContributorsState,
    UserDto,
    UsersService
} from '@app/shared';

@Injectable()
export class UsersDataSource implements AutocompleteSource {
    constructor(
        private readonly contributorsState: ContributorsState,
        private readonly usersService: UsersService
    ) {
    }

    public find(query: string): Observable<any[]> {
        return this.usersService.getUsers(query)
            .withLatestFrom(this.contributorsState.contributors.filter(x => !!x), (users, contributors) => {
                const results: any[] = [];

                for (let user of users) {
                    if (!contributors!.find(t => t.contributor.contributorId === user.id)) {
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
    templateUrl: './contributors-page.component.html',
    providers: [
        UsersDataSource
    ]
})
export class ContributorsPageComponent implements OnInit {
    public usersPermissions = [ 'Owner', 'Developer', 'Editor' ];

    public assignContributorForm = new AssignContributorForm(this.formBuilder);

    constructor(
        public readonly appsState: AppsState,
        public readonly contributorsState: ContributorsState,
        public readonly usersDataSource: UsersDataSource,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.contributorsState.load().onErrorResumeNext().subscribe();
    }

    public reload() {
        this.contributorsState.load(true).onErrorResumeNext().subscribe();
    }

    public remove(contributor: AppContributorDto) {
        this.contributorsState.revoke(contributor).onErrorResumeNext().subscribe();
    }

    public changePermission(contributor: AppContributorDto, permission: string) {
        this.contributorsState.assign(new AppContributorDto(contributor.contributorId, permission)).onErrorResumeNext().subscribe();
    }

    public assignContributor() {
        const value = this.assignContributorForm.submit();

        if (value) {
            let user = value.user;

            if (user instanceof UserDto) {
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

    public trackByContributor(index: number, contributorInfo: { contributor: AppContributorDto }) {
        return contributorInfo.contributor.contributorId;
    }
}
