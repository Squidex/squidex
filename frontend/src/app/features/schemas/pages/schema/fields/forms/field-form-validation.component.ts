/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { AppSettingsDto, FieldDto, LanguageDto, SchemaDto } from '@app/shared';

@Component({
    selector: 'sqx-field-form-validation',
    styleUrls: ['./field-form-validation.component.scss'],
    templateUrl: './field-form-validation.component.html',
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
