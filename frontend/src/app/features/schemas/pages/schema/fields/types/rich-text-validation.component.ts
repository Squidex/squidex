/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, input } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { FieldDto, FieldFormModel, FormRowComponent, RichTextFieldFormModel, RichTextFieldPropertiesDto, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-rich-text-validation',
    styleUrls: ['rich-text-validation.component.scss'],
    templateUrl: 'rich-text-validation.component.html',
    imports: [
        FormField,
        FormRowComponent,
        TranslatePipe,
    ],
})
export class RichTextValidationComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<RichTextFieldPropertiesDto>();

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<RichTextFieldFormModel>;
    });
}
