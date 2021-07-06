/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { FieldDto } from '@app/shared';

@Component({
    selector: 'sqx-field-form-ui',
    styleUrls: ['./field-form-ui.component.scss'],
    templateUrl: './field-form-ui.component.html',
})
export class FieldFormUIComponent {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;
}
