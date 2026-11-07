/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { AssetFolderDropdownComponent, FieldDto, FieldFormModel, FormRowComponent, RichTextFieldFormModel, RichTextFieldPropertiesDto, SchemaTagSource, TagEditorComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-rich-text-ui',
    styleUrls: ['rich-text-ui.component.scss'],
    templateUrl: 'rich-text-ui.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AssetFolderDropdownComponent,
        FormField,
        FormRowComponent,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class RichTextUIComponent {
    protected readonly schemasSource = inject(SchemaTagSource);

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
