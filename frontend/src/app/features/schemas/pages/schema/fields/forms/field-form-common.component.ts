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
    selector: 'sqx-field-form-common',
    styleUrls: ['./field-form-common.component.scss'],
    templateUrl: './field-form-common.component.html',
})
export class FieldFormCommonComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public schema!: SchemaDto;
}
