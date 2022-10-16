/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { AppSettingsDto, FieldDto, LanguageDto, SchemaDto } from '@app/shared';

@Component({
    selector: 'sqx-field-form-validation[field][fieldForm][languages][schema][settings]',
    styleUrls: ['./field-form-validation.component.scss'],
    templateUrl: './field-form-validation.component.html',
})
export class FieldFormValidationComponent {
    @Input()
    public fieldForm!: UntypedFormGroup;

    @Input()
    public field!: FieldDto;

    @Input()
    public schema!: SchemaDto;

    @Input()
    public settings!: AppSettingsDto;

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable?: boolean | null;
}
