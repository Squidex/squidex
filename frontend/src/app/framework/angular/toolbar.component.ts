/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ToolbarService } from '@app/framework/internal';
import { TranslatePipe } from './pipes/translate.pipe';

@Component({
    selector: 'sqx-toolbar',
    styleUrls: ['./toolbar.component.scss'],
    templateUrl: './toolbar.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        TranslatePipe,
    ],
})
export class ToolbarComponent {
    public readonly buttons;

    constructor(toolbar: ToolbarService) {
        this.buttons = toSignal(toolbar.buttonsChanges, { initialValue: [] });
    }
}
