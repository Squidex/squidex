/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { AppLanguageDto, AppSettingsDto, FieldDto, FieldFormModel, FormRowComponent, SchemaDto } from '@app/shared';
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
import { UserInfoValidationComponent } from '../types/user-info-validation.component';

@Component({
    selector: 'sqx-field-form-validation',
    styleUrls: ['./field-form-validation.component.scss'],
    templateUrl: './field-form-validation.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ArrayValidationComponent,
        AssetsValidationComponent,
        BooleanValidationComponent,
        ComponentsValidationComponent,
        ComponentValidationComponent,
        DateTimeValidationComponent,
        FormField,
        FormRowComponent,
        GeolocationValidationComponent,
        JsonValidationComponent,
        NumberValidationComponent,
        ReferencesValidationComponent,
        RichTextValidationComponent,
        StringValidationComponent,
        TagsValidationComponent,
        UserInfoValidationComponent,
    ],
})
export class FieldFormValidationComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The schema the field belongs to. */
    public readonly schema = input.required<SchemaDto>();

    /** The settings of the app. */
    public readonly settings = input.required<AppSettingsDto>();

    /** The languages of the app, used to edit localized default values. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Shows the localized default values, if the field is localizable. */
    public readonly isLocalizable = input(false, { transform: booleanAttribute });
}
