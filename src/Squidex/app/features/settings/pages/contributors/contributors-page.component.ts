/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Injectable, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Observable } from 'rxjs';
import { withLatestFrom } from 'rxjs/operators';

import {
    AppsState,
    AssignContributorForm,
    AutocompleteSource,
    ContributorDto,
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
                    if (!contributors!.find(t => t.contributorId === user.id)) {
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
        this.rolesState.load();

        this.contributorsState.load();
    }

    public reload() {
        this.contributorsState.load(true);
    }

    public remove(contributor: ContributorDto) {
        this.contributorsState.revoke(contributor);
    }

    public changeRole(contributor: ContributorDto, role: string) {
        this.contributorsState.assign({ contributorId: contributor.contributorId, role });
    }

    public assignContributor() {
        const value = this.assignContributorForm.submit();

        if (value) {
            let user = value.user;

            if (Types.is(user, UserDto)) {
                user = user.id;
            }

            const requestDto = { contributorId: user, role: 'Editor', invite: true };

            this.contributorsState.assign(requestDto)
                .subscribe(isCreated => {
                    this.assignContributorForm.submitCompleted();

                    if (isCreated) {
                        this.dialogs.notifyInfo('A new user with the entered email address has been created and assigned as contributor.');
                    }
                }, error => {
                    this.assignContributorForm.submitFailed(error);
                });
        }
    }

    public trackByContributor(index: number, contributor: ContributorDto) {
        return contributor.contributorId;
    }
}
