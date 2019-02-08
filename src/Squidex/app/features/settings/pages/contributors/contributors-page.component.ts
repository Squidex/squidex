/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Injectable, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Observable } from 'rxjs';
import { onErrorResumeNext, withLatestFrom } from 'rxjs/operators';

import {
    AppContributorDto,
    AppsState,
    AssignContributorDto,
    AssignContributorForm,
    AutocompleteSource,
    ContributorsState,
    DialogService,
    RolesState,
    Types,
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
        return this.usersService.getUsers(query).pipe(
            withLatestFrom(this.contributorsState.contributors, (users, contributors) => {
                const results: any[] = [];

                for (let user of users) {
                    if (!contributors!.find(t => t.contributor.contributorId === user.id)) {
                        results.push(user);
                    }
                }
                return results;
            }));
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
    public assignContributorForm = new AssignContributorForm(this.formBuilder);

    constructor(
        public readonly appsState: AppsState,
        public readonly contributorsState: ContributorsState,
        public readonly rolesState: RolesState,
        public readonly usersDataSource: UsersDataSource,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.rolesState.load().pipe(onErrorResumeNext()).subscribe();

        this.contributorsState.load().pipe(onErrorResumeNext()).subscribe();
    }

    public reload() {
        this.contributorsState.load(true).pipe(onErrorResumeNext()).subscribe();
    }

    public remove(contributor: AppContributorDto) {
        this.contributorsState.revoke(contributor).pipe(onErrorResumeNext()).subscribe();
    }

    public changeRole(contributor: AppContributorDto, role: string) {
        this.contributorsState.assign(new AssignContributorDto(contributor.contributorId, role)).pipe(onErrorResumeNext()).subscribe();
    }

    public assignContributor() {
        const value = this.assignContributorForm.submit();

        if (value) {
            let user = value.user;

            if (Types.is(user, UserDto)) {
                user = user.id;
            }

            const requestDto = new AssignContributorDto(user, 'Editor', true);

            this.contributorsState.assign(requestDto)
                .subscribe(wasInvited => {
                    this.assignContributorForm.submitCompleted({});

                    if (wasInvited) {
                        this.dialogs.notifyInfo('A new user with the entered email address has been created and assigned as contributor.');
                    }
                }, error => {
                    this.assignContributorForm.submitFailed(error);
                });
        }
    }

    public trackByContributor(index: number, contributorInfo: { contributor: AppContributorDto }) {
        return contributorInfo.contributor.contributorId;
    }
}
