/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, inject, Injectable, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { AssignContributorForm, AssignContributorFormModel, AutocompleteComponent, AutocompleteSource, ContributorsState, DialogModel, DialogService, FormHintComponent, ModalDirective, RoleDto, SignalSelectDirective, TranslatePipe, UserDtoPicture, UsersService } from '@app/shared';
import { ImportContributorsDialogComponent } from './import-contributors-dialog.component';

@Injectable()
export class UsersDataSource implements AutocompleteSource {
    private readonly contributorsState = inject(ContributorsState);
    private readonly usersService = inject(UsersService);


    public find(query: string): Observable<ReadonlyArray<any>> {
        if (!query) {
            return of([]);
        }

        return this.usersService.getUsers(query).pipe(
            map(users => {
                const contributors = this.contributorsState.contributors();
                const results: any[] = [];

                for (const user of users) {
                    if (!contributors.find(t => t.contributorId === user.id)) {
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
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        UsersDataSource,
    ],
    imports: [
        AutocompleteComponent,
        FormField,
        FormHintComponent,
        FormsModule,
        ImportContributorsDialogComponent,
        ModalDirective,
        SignalSelectDirective,
        TranslatePipe,
        UserDtoPicture,
    ],
})
export class ContributorAddFormComponent {
    private readonly contributorsState = inject(ContributorsState);
    private readonly dialogs = inject(DialogService);

    private readonly defaultValue = computed<Partial<AssignContributorFormModel>>(() => {
        return { role: this.roles()[0]?.name, user: '' };
    });

    protected readonly usersDataSource = inject(UsersDataSource);

    /** The roles that can be assigned to new contributors. */
    public readonly roles = input.required<ReadonlyArray<RoleDto>>();

    protected readonly assignContributorForm = new AssignContributorForm();

    protected readonly importDialog = new DialogModel();

    constructor() {
        effect(() => {
            this.assignContributorForm.submitCompleted({ newValue: this.defaultValue() });
        });
    }

    protected assignContributor() {
        const value = this.assignContributorForm.submit();
        if (!value) {
            return;
        }

        this.contributorsState.assign(value)
            .subscribe({
                next: isCreated => {
                    this.assignContributorForm.submitCompleted({ newValue: this.defaultValue() });

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
