/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { tap } from 'rxjs';
import { FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormRowComponent, ModalDialogComponent, TextInputDirective, TooltipDirective, TranslatePipe } from '@app/framework';
import { CreateTeamForm, TeamsState } from '@app/shared/internal';

@Component({
    selector: 'sqx-team-form',
    styleUrls: ['./team-form.component.scss'],
    templateUrl: './team-form.component.html',
    imports: [
        FocusOnInitDirective,
        FormAlertComponent,
        FormErrorComponent,
        FormField,
        FormRowComponent,
        FormsModule,
        ModalDialogComponent,
        TextInputDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class TeamFormComponent {
    private readonly teamsStore = inject(TeamsState);

    /** Emits when the dialog is closed or the team has been created. */
    public readonly dialogClose = output();

    protected readonly createForm = new CreateTeamForm();

    protected emitClose() {
        this.dialogClose.emit();
    }

    protected createTeam() {
        this.createForm.submit(value => {
            return this.teamsStore.create(value).pipe(tap(() => this.emitClose()));
        });
    }
}
