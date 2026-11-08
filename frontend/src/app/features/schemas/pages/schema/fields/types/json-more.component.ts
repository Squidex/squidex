/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, input } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { CodeEditorComponent, FieldDto, FieldFormModel, FormRowComponent, JsonFieldFormModel, JsonFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-json-more',
    styleUrls: ['json-more.component.scss'],
    templateUrl: 'json-more.component.html',
    imports: [
        CodeEditorComponent,
        FormField,
        FormRowComponent,
    ],
})
export class JsonMoreComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<JsonFieldPropertiesDto>();

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<JsonFieldFormModel>;
    });
}
