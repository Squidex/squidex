/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { FieldDto, FormRowComponent, RichTextFieldPropertiesDto, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-rich-text-validation',
    styleUrls: ['rich-text-validation.component.scss'],
    templateUrl: 'rich-text-validation.component.html',
    imports: [
        FormRowComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class RichTextValidationComponent  {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: RichTextFieldPropertiesDto;
}
