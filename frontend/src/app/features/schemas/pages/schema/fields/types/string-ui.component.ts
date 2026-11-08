/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, effect, inject, input, untracked } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { AssetFolderDropdownComponent, FieldDto, FieldFormModel, FormRowComponent, SchemaTagSource, StringFieldEditorValues, StringFieldFormModel, StringFieldPropertiesDto, TagEditorComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-string-ui',
    styleUrls: ['string-ui.component.scss'],
    templateUrl: 'string-ui.component.html',
    imports: [
        AssetFolderDropdownComponent,
        FormField,
        FormRowComponent,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class StringUIComponent {
    protected readonly schemasSource = inject(SchemaTagSource);

    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<StringFieldPropertiesDto>();

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<StringFieldFormModel>;
    });

    protected readonly hideAllowedValues = computed(() => {
        const editor = this.form().editor().value();

        return editor !== 'Radio' && editor !== 'Dropdown';
    });

    protected readonly hideClassNames = computed(() => {
        return this.form().editor().value() !== 'RichText';
    });

    protected readonly editors = StringFieldEditorValues;

    constructor() {
        effect(() => {
            if (this.hideAllowedValues()) {
                untracked(() => this.form().allowedValues().value.set(null));
            }
        });

        effect(() => {
            const editor = this.form().editor().value();
            if (editor !== 'Input' && editor !== 'Dropdown' && editor !== 'Slug') {
                untracked(() => this.form().inlineEditable().value.set(false));
            }
        });
    }
}
