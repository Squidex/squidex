/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, computed, input } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { DateTimeFieldEditorValues, DateTimeFieldFormModel, DateTimeFieldPropertiesDto, FieldDto, FieldFormModel, FormRowComponent } from '@app/shared';

@Component({
    selector: 'sqx-date-time-ui',
    styleUrls: ['date-time-ui.component.scss'],
    templateUrl: 'date-time-ui.component.html',
    imports: [
        FormField,
        FormRowComponent,
    ],
})
export class DateTimeUIComponent {
    protected readonly editors = DateTimeFieldEditorValues;

    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<DateTimeFieldPropertiesDto>();

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<DateTimeFieldFormModel>;
    });
}
