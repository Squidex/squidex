/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject } from '@angular/core';
import { ToolbarService } from '@app/framework/internal';
import { TranslatePipe } from './pipes/translate.pipe';

@Component({
    selector: 'sqx-toolbar',
    styleUrls: ['./toolbar.component.scss'],
    templateUrl: './toolbar.component.html',
    imports: [
        TranslatePipe,
    ],
})
export class ToolbarComponent {
    protected readonly toolbarService = inject(ToolbarService);

}
