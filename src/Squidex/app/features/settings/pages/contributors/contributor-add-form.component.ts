/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Injectable } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Observable } from 'rxjs';
import { withLatestFrom } from 'rxjs/operators';

import {
    AssignContributorForm,
    AutocompleteSource,
    ContributorsState,
    DialogModel,
    DialogService,
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
    selector: 'sqx-contributor-add-form',
    template: `
        <div class="table-items-header">
            <sqx-form-alert marginTop="0" marginBottom="2" light="true">
                Just enter the email address to invite someone with no account to the app.
            </sqx-form-alert>

            <form [formGroup]="assignContributorForm.form" (ngSubmit)="assignContributor()">
                <div class="row no-gutters">
                    <div class="col">
                        <sqx-autocomplete [source]="usersDataSource" formControlName="user" [inputName]="'contributor'" placeholder="Find existing user" displayProperty="displayName">
                            <ng-template let-user="$implicit">
                                <span class="autocomplete-user">
                                    <img class="user-picture autocomplete-user-picture" [attr.src]="user | sqxUserDtoPicture" />

                                    <span class="user-name autocomplete-user-name">{{user.displayName}}</span>
                                </span>
                            </ng-template>
                        </sqx-autocomplete>
                    </div>
                    <div class="col-auto pl-1">
                        <button type="submit" class="btn btn-success" [disabled]="assignContributorForm.hasNoUser | async">Add Contributor</button>
                    </div>
                </div>
            </form>
        </div>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContributorAddFormComponent {
    public assignContributorForm = new AssignContributorForm(this.formBuilder);

    public importDialog = new DialogModel();

    constructor(
        public readonly contributorsState: ContributorsState,
        public readonly usersDataSource: UsersDataSource,
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder
    ) {
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
}