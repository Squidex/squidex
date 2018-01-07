/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';

import { AppContext } from 'shared';

@Component({
    selector: 'sqx-settings-area',
    styleUrls: ['./settings-area.component.scss'],
    templateUrl: './settings-area.component.html',
    providers: [
        AppContext
    ]
})
export class SettingsAreaComponent {
    constructor(public readonly ctx: AppContext
    ) {
    }
}