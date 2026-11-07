/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, output, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormRowComponent, ModalDialogComponent, TooltipDirective, TransformInputDirective, TranslatePipe } from '@app/framework';
import { CreateTeamForm, TeamsState } from '@app/shared/internal';

@Component({
    selector: 'sqx-team-form',
    styleUrls: ['./team-form.component.scss'],
    templateUrl: './team-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FocusOnInitDirective,
        FormAlertComponent,
        FormErrorComponent,
        FormRowComponent,
        FormsModule,
        ModalDialogComponent,
        ReactiveFormsModule,
        TooltipDirective,
        TransformInputDirective,
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
        const value = this.createForm.submit();
        if (!value) {
            return;
        }

        this.teamsStore.create(value)
            .subscribe({
                next: () => {
                    this.emitClose();
                },
                error: error => {
                    this.createForm.submitFailed(error);
                },
            });
    }
}
