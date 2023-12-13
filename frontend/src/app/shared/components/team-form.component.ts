/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, Output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ControlErrorsComponent, FocusOnInitDirective, FormAlertComponent, FormErrorComponent, FormHintComponent, ModalDialogComponent, TooltipDirective, TransformInputDirective, TranslatePipe } from '@app/framework';
import { ApiUrlConfig, CreateTeamForm, TeamsState } from '@app/shared/internal';

@Component({
    standalone: true,
    selector: 'sqx-team-form',
    styleUrls: ['./team-form.component.scss'],
    templateUrl: './team-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ControlErrorsComponent,
        FocusOnInitDirective,
        FormAlertComponent,
        FormErrorComponent,
        FormHintComponent,
        FormsModule,
        ModalDialogComponent,
        ReactiveFormsModule,
        TooltipDirective,
        TransformInputDirective,
        TranslatePipe,
    ],
})
export class TeamFormComponent {
    @Output()
    public dialogClose = new EventEmitter();

    public createForm = new CreateTeamForm();

    constructor(
        public readonly apiUrl: ApiUrlConfig,
        private readonly teamsStore: TeamsState,
    ) {
    }

    public emitClose() {
        this.dialogClose.emit();
    }

    public createTeam() {
        const value = this.createForm.submit();

        if (value) {
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
}
