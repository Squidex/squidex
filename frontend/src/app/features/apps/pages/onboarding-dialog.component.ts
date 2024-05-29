/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { fadeAnimation, MarkdownPipe, ModalDialogComponent, SafeHtmlPipe, slideAnimation, TourState, TranslatePipe, UsersService } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-onboarding-dialog',
    styleUrls: ['./onboarding-dialog.component.scss'],
    templateUrl: './onboarding-dialog.component.html',
    animations: [
        fadeAnimation, slideAnimation,
    ],
    imports: [
        FormsModule,
        MarkdownPipe,
        ModalDialogComponent,
        ReactiveFormsModule,
        SafeHtmlPipe,
        TranslatePipe,
    ],
})
export class OnboardingDialogComponent {

    @Output()
    public dialogClose = new EventEmitter();

    public answersForm =
        this.formBuilder.group({
            companySize: '',
            companyRole: '',
            project: '',
        });

    public step = 0;

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly tourState: TourState,
        private readonly usersService: UsersService,
    ) {
    }

    public submitAnswers() {
        this.usersService.postUser({ answers: this.answersForm.value as any }).subscribe();
        this.next();
    }

    public start() {
        this.tourState.start();
        this.dialogClose.emit();
    }

    public cancel() {
        this.tourState.complete();
        this.dialogClose.emit();
    }

    public next() {
        this.step += 1;
    }
}
