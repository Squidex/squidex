/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { ArrayFieldPropertiesDto, FieldDto } from '@app/shared';

@Component({
    selector: 'sqx-array-validation[field][fieldForm][properties]',
    styleUrls: ['array-validation.component.scss'],
    templateUrl: 'array-validation.component.html',
})
export class ArrayValidationComponent {
    @Input()
    public fieldForm!: UntypedFormGroup;

    @Input()
    public field!: FieldDto;

    @Input()
    public properties!: ArrayFieldPropertiesDto;
}
