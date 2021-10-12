/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { FieldDto, SchemaDto } from '@app/shared';

@Component({
    selector: 'sqx-field-form-common[field][fieldForm][schema]',
    styleUrls: ['./field-form-common.component.scss'],
    templateUrl: './field-form-common.component.html',
})
export class FieldFormCommonComponent {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public schema: SchemaDto;
}
