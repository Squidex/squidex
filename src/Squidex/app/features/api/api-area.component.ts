/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';

import { AppContext } from 'shared';

@Component({
    selector: 'sqx-api-area',
    styleUrls: ['./api-area.component.scss'],
    templateUrl: './api-area.component.html',
    providers: [
        AppContext
    ]
})
export class ApiAreaComponent {
    constructor(
        public readonly ctx: AppContext
    ) {
    }
}