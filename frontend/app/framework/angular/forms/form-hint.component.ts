/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
    selector: 'sqx-form-hint',
    styleUrls: ['./form-hint.component.scss'],
    templateUrl: './form-hint.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormHintComponent {
    @Input()
    public class: string;

    @Input()
    public marginTop = 0;

    @Input()
    public marginBottom = 0;
}