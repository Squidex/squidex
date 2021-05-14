/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
    selector: 'sqx-form-alert',
    styleUrls: ['./form-alert.component.scss'],
    templateUrl: './form-alert.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormAlertComponent {
    @Input()
    public class: string;

    @Input()
    public marginTop: number | string | undefined | null = 2;

    @Input()
    public marginBottom: number | string | undefined | null = 4;

    @Input()
    public light?: boolean | null;
}
