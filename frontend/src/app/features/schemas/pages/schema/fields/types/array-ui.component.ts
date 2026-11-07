/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { ArrayFieldPropertiesDto, FieldDto, FormRowComponent } from '@app/shared';

const CALCULATED_DEFAULT_VALUES: ReadonlyArray<string> = ['EmptyArray', 'Null'];

@Component({
    selector: 'sqx-array-ui',
    styleUrls: ['array-ui.component.scss'],
    templateUrl: 'array-ui.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormRowComponent,
        FormsModule,
        ReactiveFormsModule,
    ],
})
export class ArrayUIComponent {
    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<ArrayFieldPropertiesDto>();

    protected readonly calculatedDefaultValues = CALCULATED_DEFAULT_VALUES;
}
