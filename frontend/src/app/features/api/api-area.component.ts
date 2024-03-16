/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { AppsState } from '@app/shared';

@Component({
    selector: 'sqx-api-area',
    styleUrls: ['./api-area.component.scss'],
    templateUrl: './api-area.component.html',
})
export class ApiAreaComponent {
    constructor(
        public readonly appsState: AppsState,
    ) {
    }
}
