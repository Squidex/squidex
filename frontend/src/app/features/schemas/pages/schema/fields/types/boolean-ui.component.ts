/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { BooleanFieldEditorValues, BooleanFieldPropertiesDto, FieldDto, FormRowComponent } from '@app/shared';

@Component({
    selector: 'sqx-boolean-ui',
    styleUrls: ['boolean-ui.component.scss'],
    templateUrl: 'boolean-ui.component.html',
    imports: [
        FormRowComponent,
        FormsModule,
        ReactiveFormsModule,
    ],
})
export class BooleanUIComponent {
    protected readonly editors = BooleanFieldEditorValues;

    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<BooleanFieldPropertiesDto>();
}
