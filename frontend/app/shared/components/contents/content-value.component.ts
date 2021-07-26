/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { HtmlValue, Types } from '@app/shared/internal';

@Component({
    selector: 'sqx-content-value[value]',
    styleUrls: ['./content-value.component.scss'],
    templateUrl: './content-value.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ContentValueComponent {
    @Input()
    public value: any;

    public get isPlain() {
        return !Types.is(this.value, HtmlValue);
    }
}
