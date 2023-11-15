/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgFor } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { ArrayFieldPropertiesDto, FieldDto, TranslatePipe } from '@app/shared';

const CALCULATED_DEFAULT_VALUES: ReadonlyArray<string> = ['EmptyArray', 'Null'];

@Component({
    selector: 'sqx-array-ui',
    styleUrls: ['array-ui.component.scss'],
    templateUrl: 'array-ui.component.html',
    standalone: true,
    imports: [
        FormsModule,
        ReactiveFormsModule,
        NgFor,
        TranslatePipe,
    ],
})
export class ArrayUIComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: ArrayFieldPropertiesDto;

    public calculatedDefaultValues = CALCULATED_DEFAULT_VALUES;
}
