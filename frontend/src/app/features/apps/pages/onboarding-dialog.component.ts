/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { form, FormField } from '@angular/forms/signals';
import { FormRowComponent, MarkdownDirective, ModalDialogComponent, SignalSelectDirective, TourState, TranslatePipe, UsersService } from '@app/shared';

@Component({
    selector: 'sqx-onboarding-dialog',
    styleUrls: ['./onboarding-dialog.component.scss'],
    templateUrl: './onboarding-dialog.component.html',
    imports: [
        FormField,
        FormRowComponent,
        FormsModule,
        MarkdownDirective,
        ModalDialogComponent,
        SignalSelectDirective,
        TranslatePipe,
    ],
})
export class OnboardingDialogComponent {
    private readonly tourState = inject(TourState);
    private readonly usersService = inject(UsersService);

    /** Emits when the user closes the dialog, by skipping or starting the tour. */
    public readonly dialogClose = output();

    protected readonly answersForm = form(signal({ companySize: '', companyRole: '', project: '' }));

    protected readonly step = signal(0);

    protected submitAnswers() {
        this.usersService.postUser({ answers: this.answersForm().value() }).subscribe();
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
