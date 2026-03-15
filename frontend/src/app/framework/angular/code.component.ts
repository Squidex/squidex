/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MathHelper } from '../internal';
import { TranslatePipe } from './pipes/translate.pipe';

@Component({
    selector: 'sqx-code',
    styleUrls: ['./code.component.scss'],
    templateUrl: './code.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        TranslatePipe,
    ],
})
export class CodeComponent {
    public readonly id = MathHelper.guid();
}
