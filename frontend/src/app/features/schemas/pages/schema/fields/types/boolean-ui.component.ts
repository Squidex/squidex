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
    selector: 'sqx-boolean-ui',
    styleUrls: ['boolean-ui.component.scss'],
    templateUrl: 'boolean-ui.component.html',
    standalone: true,
    imports: [
        FormsModule,
        ReactiveFormsModule,
        FormHintComponent,
        NgFor,
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
