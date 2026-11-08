/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject, Service } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { Observable, of } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { AutocompleteComponent, AutocompleteSource, DialogModel, DialogService, FormHintComponent, ModalDirective, TranslatePipe, UsersService } from '@app/shared';
import { UserDtoPicture } from '@app/shared';
import { AssignTeamContributorForm } from '../../internal';
import { TeamContributorsState } from '../../state/team-contributors.state';
import { ImportContributorsDialogComponent } from './import-contributors-dialog.component';

@Service({ autoProvided: false })
export class UsersDataSource implements AutocompleteSource {
    private readonly contributorsState = inject(TeamContributorsState);
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
        TranslatePipe,
        UserDtoPicture,
    ],
})
export class ContributorAddFormComponent {
    private readonly contributorsState = inject(TeamContributorsState);
    private readonly dialogs = inject(DialogService);

    protected readonly usersDataSource = inject(UsersDataSource);

    protected readonly assignContributorForm = new AssignTeamContributorForm();

    protected readonly importDialog = new DialogModel();

    protected assignContributor() {
        this.assignContributorForm.submit(value => {
            return this.contributorsState.assign(value).pipe(
                tap(isCreated => {
                    if (isCreated) {
                        this.dialogs.notifyInfo('i18n:contributors.contributorAssigned');
                    } else {
                        this.dialogs.notifyInfo('i18n:contributors.contributorAssignedOld');
                    }
                }));
        }, { resetAfterSubmit: true });
    }
}
