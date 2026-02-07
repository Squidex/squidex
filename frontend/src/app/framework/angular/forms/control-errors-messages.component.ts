/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */


import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
    selector: 'sqx-errors-messages',
    styleUrls: ['./control-errors-messages.component.scss'],
    templateUrl: './control-errors-messages.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ControlErrorsMessagesComponent {
    @Input()
    public errorMessages?: ReadonlyArray<string>;
}
