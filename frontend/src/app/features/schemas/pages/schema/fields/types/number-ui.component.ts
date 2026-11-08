/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, input } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { FieldDto, FieldFormModel, FloatConverter, FormRowComponent, NumberFieldEditorValues, NumberFieldFormModel, NumberFieldPropertiesDto, TagEditorComponent } from '@app/shared';

@Component({
    selector: 'sqx-number-ui',
    styleUrls: ['number-ui.component.scss'],
    templateUrl: 'number-ui.component.html',
    imports: [
        FormField,
        FormRowComponent,
        TagEditorComponent,
    ],
})
export class NumberUIComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<NumberFieldPropertiesDto>();

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<NumberFieldFormModel>;
    });

    protected readonly converter = FloatConverter.INSTANCE;

    protected readonly editors = NumberFieldEditorValues;
}
