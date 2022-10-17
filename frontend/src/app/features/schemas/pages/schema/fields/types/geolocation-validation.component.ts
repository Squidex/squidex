/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { FieldDto, GeolocationFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-geolocation-validation[field][fieldForm][properties]',
    styleUrls: ['geolocation-validation.component.scss'],
    templateUrl: 'geolocation-validation.component.html',
})
export class GeolocationValidationComponent {
    @Input()
    public fieldForm!: UntypedFormGroup;

    @Input()
    public field!: FieldDto;

    @Input()
    public properties!: GeolocationFieldPropertiesDto;
}
