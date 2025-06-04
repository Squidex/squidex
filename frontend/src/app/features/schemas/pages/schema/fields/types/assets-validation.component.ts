/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { AppLanguageDto, AssetsFieldPropertiesDto, FieldDto, FormHintComponent, LocalizedInputComponent, TagEditorComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-assets-validation',
    styleUrls: ['assets-validation.component.scss'],
    templateUrl: 'assets-validation.component.html',
    imports: [
        FormHintComponent,
        FormsModule,
        LocalizedInputComponent,
        ReactiveFormsModule,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class AssetsValidationComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: AssetsFieldPropertiesDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    @Input({ transform: booleanAttribute })
    public isLocalizable?: boolean | null;
}
