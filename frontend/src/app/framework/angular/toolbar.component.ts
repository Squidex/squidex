/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ToolbarService } from '@app/framework/internal';

@Component({
    selector: 'sqx-toolbar',
    styleUrls: ['./toolbar.component.scss'],
    templateUrl: './toolbar.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToolbarComponent {
    constructor(
        public readonly toolbar: ToolbarService,
    ) {
    }
}
