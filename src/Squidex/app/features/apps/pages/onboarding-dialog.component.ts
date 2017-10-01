/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input } from '@angular/core';

import {
    fadeAnimation,
    ModalView,
    slideAnimation
} from 'framework';

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

    @Input()
    public modalView = new ModalView();

    public next() {
        this.step = this.step + 1;
    }
}