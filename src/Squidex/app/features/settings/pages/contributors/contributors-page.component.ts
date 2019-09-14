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
    DialogModel,
    DialogService,
    RolesState,
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

    public importDialog = new DialogModel();

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

    public goPrev() {
        this.contributorsState.goPrev();
    }

    public goNext() {
        this.contributorsState.goNext();
    }

    public search(query: string) {
        this.contributorsState.search(query);
    }

    public assignContributor() {
        const value = this.assignContributorForm.submit();

        if (value) {
            this.contributorsState.assign(value)
                .subscribe(isCreated => {
                    this.assignContributorForm.submitCompleted();

                    if (isCreated) {
                        this.dialogs.notifyInfo('A new user with the entered email address has been created and assigned as contributor.');
                    } else {
                        this.dialogs.notifyInfo('User has been added as contributor.');
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
