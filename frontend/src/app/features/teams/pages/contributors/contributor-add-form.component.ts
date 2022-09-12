/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { withLatestFrom } from 'rxjs/operators';
import { AssignTeamContributorForm, TeamContributorsState } from '@app/features/teams/internal';
import { AutocompleteSource, DialogModel, DialogService, UsersService } from '@app/shared';

@Injectable()
export class UsersDataSource implements AutocompleteSource {
    constructor(
        private readonly contributorsState: TeamContributorsState,
        private readonly usersService: UsersService,
    ) {
    }

    public find(query: string): Observable<ReadonlyArray<any>> {
        if (!query) {
            return of([]);
        }

        return this.usersService.getUsers(query).pipe(
            withLatestFrom(this.contributorsState.contributors, (users, contributors) => {
                const results: any[] = [];

                for (const user of users) {
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
    styleUrls: ['./contributor-add-form.component.scss'],
    templateUrl: './contributor-add-form.component.html',
    providers: [
        UsersDataSource,
    ],
})
export class ContributorAddFormComponent {
    public assignContributorForm = new AssignTeamContributorForm();

    public importDialog = new DialogModel();

    constructor(
        public readonly contributorsState: TeamContributorsState,
        public readonly usersDataSource: UsersDataSource,
        private readonly dialogs: DialogService,
    ) {
    }

    public assignContributor() {
        const value = this.assignContributorForm.submit();

        if (value) {
            this.contributorsState.assign(value)
                .subscribe({
                    next: isCreated => {
                        this.assignContributorForm.submitCompleted({ newValue: { user: '' } as any });

                        if (isCreated) {
                            this.dialogs.notifyInfo('i18n:contributors.contributorAssigned');
                        } else {
                            this.dialogs.notifyInfo('i18n:contributors.contributorAssignedOld');
                        }
                    },
                    error: error => {
                        this.assignContributorForm.submitFailed(error);
                    },
                });
        }
    }
}
