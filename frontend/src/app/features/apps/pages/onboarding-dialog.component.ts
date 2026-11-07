/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, output, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { FormRowComponent, MarkdownDirective, ModalDialogComponent, TourState, TranslatePipe, UsersService } from '@app/shared';

@Component({
    selector: 'sqx-onboarding-dialog',
    styleUrls: ['./onboarding-dialog.component.scss'],
    templateUrl: './onboarding-dialog.component.html',
    imports: [
        FormRowComponent,
        FormsModule,
        MarkdownDirective,
        ModalDialogComponent,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class OnboardingDialogComponent {
    /** Emits when the user closes the dialog, by skipping or starting the tour. */
    public readonly dialogClose = output();

    protected readonly answersForm =
        this.formBuilder.group({
            companySize: '',
            companyRole: '',
            project: '',
        });

    protected readonly step = signal(0);

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly tourState: TourState,
        private readonly usersService: UsersService,
    ) {
    }

    protected submitAnswers() {
        this.usersService.postUser({ answers: this.answersForm.value as any }).subscribe();
        this.next();
    }

    protected start() {
        this.tourState.start();
        this.dialogClose.emit();
    }

    protected cancel() {
        this.tourState.complete();
        this.dialogClose.emit();
    }

    protected next() {
        this.step.update(x => x + 1);
    }
}
