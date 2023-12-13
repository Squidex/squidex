/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { FieldDto, ReferencesFieldPropertiesDto, SchemaTagSource, TagEditorComponent, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-component-validation',
    styleUrls: ['component-validation.component.scss'],
    templateUrl: 'component-validation.component.html',
    imports: [
        AsyncPipe,
        FormsModule,
        ReactiveFormsModule,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class ComponentValidationComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: ReferencesFieldPropertiesDto;

    constructor(
        public readonly schemasSource: SchemaTagSource,
    ) {
    }
}
