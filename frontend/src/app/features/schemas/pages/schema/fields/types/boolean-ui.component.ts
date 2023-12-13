/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgFor } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { BOOLEAN_FIELD_EDITORS, BooleanFieldPropertiesDto, FieldDto, FormHintComponent, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-boolean-ui',
    styleUrls: ['boolean-ui.component.scss'],
    templateUrl: 'boolean-ui.component.html',
    imports: [
        FormHintComponent,
        FormsModule,
        NgFor,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class BooleanUIComponent {
    public readonly editors = BOOLEAN_FIELD_EDITORS;

    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: BooleanFieldPropertiesDto;
}
