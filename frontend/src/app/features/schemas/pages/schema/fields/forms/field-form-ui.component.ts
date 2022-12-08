/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { FieldDto, SchemaDto } from '@app/shared';

@Component({
    selector: 'sqx-field-form-ui[field][fieldForm][schema]',
    styleUrls: ['./field-form-ui.component.scss'],
    templateUrl: './field-form-ui.component.html',
})
export class FieldFormUIComponent {
    @Input()
    public fieldForm!: UntypedFormGroup;

    @Input()
    public field!: FieldDto;

    @Input()
    public schema!: SchemaDto;
}
