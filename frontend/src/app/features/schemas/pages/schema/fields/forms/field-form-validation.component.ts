/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { AppSettingsDto, FieldDto, LanguageDto, SchemaDto, TranslatePipe } from '@app/shared';
import { ArrayValidationComponent } from '../types/array-validation.component';
import { AssetsValidationComponent } from '../types/assets-validation.component';
import { BooleanValidationComponent } from '../types/boolean-validation.component';
import { ComponentValidationComponent } from '../types/component-validation.component';
import { ComponentsValidationComponent } from '../types/components-validation.component';
import { DateTimeValidationComponent } from '../types/date-time-validation.component';
import { GeolocationValidationComponent } from '../types/geolocation-validation.component';
import { JsonValidationComponent } from '../types/json-validation.component';
import { NumberValidationComponent } from '../types/number-validation.component';
import { ReferencesValidationComponent } from '../types/references-validation.component';
import { RichTextValidationComponent } from '../types/rich-text-validation.component';
import { StringValidationComponent } from '../types/string-validation.component';
import { TagsValidationComponent } from '../types/tags-validation.component';

@Component({
    standalone: true,
    selector: 'sqx-field-form-validation',
    styleUrls: ['./field-form-validation.component.scss'],
    templateUrl: './field-form-validation.component.html',
    imports: [
        ArrayValidationComponent,
        AssetsValidationComponent,
        BooleanValidationComponent,
        ComponentValidationComponent,
        ComponentsValidationComponent,
        DateTimeValidationComponent,
        FormsModule,
        GeolocationValidationComponent,
        JsonValidationComponent,
        NumberValidationComponent,
        ReactiveFormsModule,
        ReferencesValidationComponent,
        RichTextValidationComponent,
        StringValidationComponent,
        TagsValidationComponent,
        TranslatePipe,
    ],
})
export class FieldFormValidationComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public schema!: SchemaDto;

    @Input({ required: true })
    public settings!: AppSettingsDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    @Input({ transform: booleanAttribute })
    public isLocalizable?: boolean | null;
}
