/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { UIState } from '@app/shared';

@Component({
    selector: 'sqx-administration-area',
    styleUrls: ['./administration-area.component.scss'],
    templateUrl: './administration-area.component.html',
})
export class AdministrationAreaComponent {
    constructor(
        public readonly uiState: UIState,
    ) {
    }
}
