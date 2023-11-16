/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, Input, numberAttribute } from '@angular/core';

@Component({
    standalone: true,
    selector: 'sqx-form-alert',
    styleUrls: ['./form-alert.component.scss'],
    templateUrl: './form-alert.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormAlertComponent {
    @Input()
    public class = '';

    @Input({ transform: numberAttribute })
    public marginTop: number | undefined | null = 2;

    @Input({ transform: numberAttribute })
    public marginBottom: number | undefined | null = 4;

    @Input({ transform: booleanAttribute })
    public light?: boolean | null;

    @Input({ transform: booleanAttribute })
    public large?: boolean | null;
}
