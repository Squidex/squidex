/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { AssetsFieldPropertiesDto, FieldDto, LanguageDto } from '@app/shared';

@Component({
    selector: 'sqx-assets-validation[field][fieldForm][languages][properties]',
    styleUrls: ['assets-validation.component.scss'],
    templateUrl: 'assets-validation.component.html',
})
export class AssetsValidationComponent {
    @Input()
    public fieldForm!: UntypedFormGroup;

    @Input()
    public field!: FieldDto;

    @Input()
    public properties!: AssetsFieldPropertiesDto;

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable?: boolean | null;
}
