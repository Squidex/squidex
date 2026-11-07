/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, effect, input } from '@angular/core';
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

    protected readonly hideAllowedValues = computed(() => {
        return hidesAllowedValues(this.form().editor().value());
    });

    protected readonly converter = FloatConverter.INSTANCE;

    protected readonly editors = NumberFieldEditorValues;

    constructor() {
        effect(() => {
            const form = this.form();
            if (hidesAllowedValues(form.editor().value())) {
                form.allowedValues().value.set(null);
            }
        });

        effect(() => {
            const form = this.form();
            if (hidesInlineEditable(form.editor().value())) {
                form.inlineEditable().value.set(false);
            }
        });
    }
}

function hidesAllowedValues(editor: string) {
    return !(editor && (editor === 'Radio' || editor === 'Dropdown'));
}

function hidesInlineEditable(editor: string) {
    return editor === 'Radio';
}
