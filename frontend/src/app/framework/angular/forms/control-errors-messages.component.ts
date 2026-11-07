/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */


import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
    selector: 'sqx-errors-messages',
    styleUrls: ['./control-errors-messages.component.scss'],
    templateUrl: './control-errors-messages.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ControlErrorsMessagesComponent {
    /** The error messages to render. Nothing is rendered when empty. */
    public readonly errorMessages = input<ReadonlyArray<string>>();
}
