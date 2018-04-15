/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Output } from '@angular/core';

import { fadeAnimation, slideAnimation } from '@app/framework';

@Component({
    selector: 'sqx-onboarding-dialog',
    styleUrls: ['./onboarding-dialog.component.scss'],
    templateUrl: './onboarding-dialog.component.html',
    animations: [
        fadeAnimation, slideAnimation
    ]
})
export class OnboardingDialogComponent {
    public step = 0;

    @Output()
    public closed = new EventEmitter();

    public close() {
        this.closed.emit();
    }

    public next() {
        this.step = this.step + 1;
    }
}