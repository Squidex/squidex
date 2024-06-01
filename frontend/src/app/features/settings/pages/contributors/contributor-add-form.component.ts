/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, Injectable, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { withLatestFrom } from 'rxjs/operators';
import { AssignContributorForm, AutocompleteComponent, AutocompleteSource, ContributorsState, DialogModel, DialogService, FormHintComponent, ModalDirective, RoleDto, TranslatePipe, UserDtoPicture, UsersService } from '@app/shared';
import { ImportContributorsDialogComponent } from './import-contributors-dialog.component';

@Injectable()
export class UsersDataSource implements AutocompleteSource {
    constructor(
        private readonly contributorsState: ContributorsState,
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
    standalone: true,
    selector: 'sqx-contributor-add-form',
    styleUrls: ['./contributor-add-form.component.scss'],
    templateUrl: './contributor-add-form.component.html',
    providers: [
        UsersDataSource,
    ],
    imports: [
        AsyncPipe,
        AutocompleteComponent,
        FormHintComponent,
        FormsModule,
        ImportContributorsDialogComponent,
        ModalDirective,
        ReactiveFormsModule,
        TranslatePipe,
        UserDtoPicture,
    ],
})
export class ContributorAddFormComponent {
    private defaultValue: any;

    @Input({ required: true })
    public roles!: ReadonlyArray<RoleDto>;

    public assignContributorForm = new AssignContributorForm();

    public importDialog = new DialogModel();

    constructor(
        public readonly contributorsState: ContributorsState,
        public readonly usersDataSource: UsersDataSource,
        private readonly dialogs: DialogService,
    ) {
    }

    public ngOnChanges() {
        this.defaultValue = { role: this.roles?.[0]?.name, user: '' };

        this.assignContributorForm.submitCompleted({ newValue: this.defaultValue });
    }

    public assignContributor() {
        const value = this.assignContributorForm.submit();

        if (value) {
            this.contributorsState.assign(value)
                .subscribe({
                    next: isCreated => {
                        this.assignContributorForm.submitCompleted({ newValue: this.defaultValue });

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
