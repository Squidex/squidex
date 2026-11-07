/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, effect, inject, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
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
        FormsModule,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class StringUIComponent {
    protected readonly schemasSource = inject(SchemaTagSource);

    protected readonly editors = StringFieldEditorValues;

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
        return hidesAllowedValues(this.form().editor().value());
    });

    protected readonly hideClassNames = computed(() => {
        return hidesClassNames(this.form().editor().value());
    });

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

function hidesClassNames(editor: string) {
    return !(editor && editor === 'RichText');
}

function hidesInlineEditable(editor: string) {
    return !(editor && (editor === 'Input' || editor === 'Dropdown' || editor === 'Slug'));
}
