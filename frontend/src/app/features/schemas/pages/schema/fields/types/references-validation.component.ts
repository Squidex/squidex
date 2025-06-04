/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { AppLanguageDto, FieldDto, FormHintComponent, LocalizedInputComponent, ReferencesFieldPropertiesDto, SchemaTagSource, TagEditorComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-references-validation',
    styleUrls: ['references-validation.component.scss'],
    templateUrl: 'references-validation.component.html',
    imports: [
        AsyncPipe,
        FormHintComponent,
        FormsModule,
        LocalizedInputComponent,
        ReactiveFormsModule,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class ReferencesValidationComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: ReferencesFieldPropertiesDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    @Input({ transform: booleanAttribute })
    public isLocalizable?: boolean | null;

    constructor(
        public readonly schemasSource: SchemaTagSource,
    ) {
    }
}
