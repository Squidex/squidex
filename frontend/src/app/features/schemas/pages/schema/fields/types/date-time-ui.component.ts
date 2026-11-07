/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { DateTimeFieldEditorValues, DateTimeFieldPropertiesDto, FieldDto, FormRowComponent } from '@app/shared';

@Component({
    selector: 'sqx-date-time-ui',
    styleUrls: ['date-time-ui.component.scss'],
    templateUrl: 'date-time-ui.component.html',
    imports: [
        FormRowComponent,
        FormsModule,
        ReactiveFormsModule,
    ],
})
export class DateTimeUIComponent {
    protected readonly editors = DateTimeFieldEditorValues;

    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<DateTimeFieldPropertiesDto>();
}
