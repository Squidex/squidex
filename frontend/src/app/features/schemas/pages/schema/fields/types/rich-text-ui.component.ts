/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { FieldDto, FormHintComponent, RichTextFieldPropertiesDto, SchemaTagSource, TagEditorComponent, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-rich-text-ui',
    styleUrls: ['rich-text-ui.component.scss'],
    templateUrl: 'rich-text-ui.component.html',
    imports: [
        AsyncPipe,
        FormHintComponent,
        FormsModule,
        ReactiveFormsModule,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class RichTextUIComponent  {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: RichTextFieldPropertiesDto;

    constructor(
        public readonly schemasSource: SchemaTagSource,
    ) {
    }
}
