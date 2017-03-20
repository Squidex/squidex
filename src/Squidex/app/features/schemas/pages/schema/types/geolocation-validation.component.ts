/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { GeolocationFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-geolocation-validation',
    styleUrls: ['geolocation-validation.component.scss'],
    templateUrl: 'geolocation-validation.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class GeolocationValidationComponent {
    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: GeolocationFieldPropertiesDto;
}