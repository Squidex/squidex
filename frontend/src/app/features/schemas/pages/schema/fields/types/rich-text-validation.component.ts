/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { FieldDto, RichTextFieldPropertiesDto } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-rich-text-validation',
    styleUrls: ['rich-text-validation.component.scss'],
    templateUrl: 'rich-text-validation.component.html',
    imports: [
        ReactiveFormsModule,
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
