/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, input } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { FieldDto, FieldFormModel, FormRowComponent, SchemaDto, SignalSelectDirective, TagEditorComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-field-form-common',
    styleUrls: ['./field-form-common.component.scss'],
    templateUrl: './field-form-common.component.html',
    imports: [
        FormField,
        FormRowComponent,
        SignalSelectDirective,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class FieldFormCommonComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The schema the field belongs to. */
    public readonly schema = input.required<SchemaDto>();
}
