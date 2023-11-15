/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgFor } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { FieldDto, ReferencesFieldPropertiesDto, TranslatePipe } from '@app/shared';

const CALCULATED_DEFAULT_VALUES: ReadonlyArray<string> = ['EmptyArray', 'Null'];

@Component({
    selector: 'sqx-components-ui',
    styleUrls: ['components-ui.component.scss'],
    templateUrl: 'components-ui.component.html',
    standalone: true,
    imports: [
        FormsModule,
        ReactiveFormsModule,
        NgFor,
        TranslatePipe,
    ],
})
export class ComponentsUIComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: ReferencesFieldPropertiesDto;

    public calculatedDefaultValues = CALCULATED_DEFAULT_VALUES;
}
