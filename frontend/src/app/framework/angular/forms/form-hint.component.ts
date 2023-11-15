/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, numberAttribute } from '@angular/core';

@Component({
    standalone: true,
    selector: 'sqx-form-hint',
    styleUrls: ['./form-hint.component.scss'],
    templateUrl: './form-hint.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormHintComponent {
    @Input()
    public class = '';

    @Input({ transform: numberAttribute })
    public marginTop: number | string = 0;

    @Input({ transform: numberAttribute })
    public marginBottom: number | string = 0;
}
