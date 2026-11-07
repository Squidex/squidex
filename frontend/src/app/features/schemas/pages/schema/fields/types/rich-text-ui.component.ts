/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { AssetFolderDropdownComponent, FieldDto, FormRowComponent, RichTextFieldPropertiesDto, SchemaTagSource, TagEditorComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-rich-text-ui',
    styleUrls: ['rich-text-ui.component.scss'],
    templateUrl: 'rich-text-ui.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AssetFolderDropdownComponent,
        AsyncPipe,
        FormRowComponent,
        FormsModule,
        ReactiveFormsModule,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class RichTextUIComponent {
    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<RichTextFieldPropertiesDto>();

    constructor(
        public readonly schemasSource: SchemaTagSource,
    ) {
    }
}
