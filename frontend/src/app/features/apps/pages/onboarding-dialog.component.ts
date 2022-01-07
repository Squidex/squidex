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
        fadeAnimation, slideAnimation,
    ],
})
export class OnboardingDialogComponent {
    public step = 0;

    @Output()
    public close = new EventEmitter();

    public next() {
        this.step += 1;
    }
}
