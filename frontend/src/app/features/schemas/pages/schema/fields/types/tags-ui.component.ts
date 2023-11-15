/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgFor } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { FieldDto, FormHintComponent, TagEditorComponent, TAGS_FIELD_EDITORS, TagsFieldPropertiesDto, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-tags-ui',
    styleUrls: ['tags-ui.component.scss'],
    templateUrl: 'tags-ui.component.html',
    standalone: true,
    imports: [
        FormsModule,
        ReactiveFormsModule,
        FormHintComponent,
        NgFor,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class TagsUIComponent {
    public readonly editors = TAGS_FIELD_EDITORS;

    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: TagsFieldPropertiesDto;
}
