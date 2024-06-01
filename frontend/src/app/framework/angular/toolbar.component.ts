/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ToolbarService } from '@app/framework/internal';
import { TranslatePipe } from './pipes/translate.pipe';

@Component({
    standalone: true,
    selector: 'sqx-toolbar',
    styleUrls: ['./toolbar.component.scss'],
    templateUrl: './toolbar.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        TranslatePipe,
    ],
})
export class ToolbarComponent {
    constructor(
        public readonly toolbar: ToolbarService,
    ) {
    }
}
