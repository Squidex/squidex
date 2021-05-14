/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { AppSettingsDto, FieldDto, LanguageDto } from '@app/shared';

@Component({
    selector: 'sqx-field-form-validation',
    styleUrls: ['./field-form-validation.component.scss'],
    templateUrl: './field-form-validation.component.html',
})
export class FieldFormValidationComponent {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public settings: AppSettingsDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable?: boolean | null;
}
