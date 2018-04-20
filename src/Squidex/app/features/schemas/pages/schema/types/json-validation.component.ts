/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { FieldDto, JsonFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-json-validation',
    styleUrls: ['json-validation.component.scss'],
    templateUrl: 'json-validation.component.html'
})
export class JsonValidationComponent {
    @Input()
    public editForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: JsonFieldPropertiesDto;
}