/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgIf } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { ControlErrorsComponent, FieldDto, FormHintComponent, SchemaDto, TagEditorComponent, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-field-form-common',
    styleUrls: ['./field-form-common.component.scss'],
    templateUrl: './field-form-common.component.html',
    imports: [
        ControlErrorsComponent,
        FormHintComponent,
        FormsModule,
        NgIf,
        ReactiveFormsModule,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class FieldFormCommonComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public schema!: SchemaDto;
}
