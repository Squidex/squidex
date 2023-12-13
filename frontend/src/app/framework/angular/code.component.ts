/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MathHelper } from '../internal';

@Component({
    standalone: true,
    selector: 'sqx-code',
    styleUrls: ['./code.component.scss'],
    templateUrl: './code.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CodeComponent {
    public readonly id = MathHelper.guid();
}
