/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { CodeEditorComponent, FieldDto, FormHintComponent, JsonFieldPropertiesDto, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-json-more',
    styleUrls: ['json-more.component.scss'],
    templateUrl: 'json-more.component.html',
    imports: [
        CodeEditorComponent,
        FormHintComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class JsonMoreComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: JsonFieldPropertiesDto;
}
