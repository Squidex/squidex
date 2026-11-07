/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { FieldDto, FormRowComponent, TagEditorComponent, TagsFieldEditorValues, TagsFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-tags-ui',
    styleUrls: ['tags-ui.component.scss'],
    templateUrl: 'tags-ui.component.html',
    imports: [
        FormRowComponent,
        FormsModule,
        ReactiveFormsModule,
        TagEditorComponent,
    ],
})
export class TagsUIComponent {
    protected readonly editors = TagsFieldEditorValues;

    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<TagsFieldPropertiesDto>();
}
