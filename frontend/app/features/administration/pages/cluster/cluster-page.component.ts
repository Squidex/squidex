/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { UIState } from '@app/shared';

@Component({
    selector: 'sqx-cluster-area',
    styleUrls: ['./cluster-page.component.scss'],
    templateUrl: './cluster-page.component.html'
})
export class ClusterPageComponent {
    constructor(
        public readonly uiState: UIState
    ) {
    }
}