/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */

import { NgFor, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { fadeAnimation } from '@app/framework/internal';

@Component({
    standalone: true,
    selector: 'sqx-errors-messages',
    styleUrls: ['./control-errors-messages.component.scss'],
    templateUrl: './control-errors-messages.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        NgFor,
        NgIf,
    ],
})
export class ControlErrorsMessagesComponent {
    @Input()
    public errorMessages?: ReadonlyArray<string>;
}
