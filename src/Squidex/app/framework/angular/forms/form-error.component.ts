/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

import { fadeAnimation } from './../animations';

@Component({
    selector: 'sqx-form-error',
    styleUrls: ['./form-error.component.scss'],
    templateUrl: './form-error.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormErrorComponent {
    @Input()
    public error: string;
}