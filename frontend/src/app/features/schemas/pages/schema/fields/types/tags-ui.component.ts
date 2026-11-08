/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, computed, input } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { FieldDto, FieldFormModel, FormRowComponent, TagEditorComponent, TagsFieldEditorValues, TagsFieldFormModel, TagsFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-tags-ui',
    styleUrls: ['tags-ui.component.scss'],
    templateUrl: 'tags-ui.component.html',
    imports: [
        FormField,
        FormRowComponent,
        TagEditorComponent,
    ],
})
export class TagsUIComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<TagsFieldPropertiesDto>();

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<TagsFieldFormModel>;
    });

    protected readonly editors = TagsFieldEditorValues;
}
