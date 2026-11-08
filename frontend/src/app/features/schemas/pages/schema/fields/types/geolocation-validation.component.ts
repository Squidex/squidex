/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, input } from '@angular/core';
import { FieldTree } from '@angular/forms/signals';
import { FieldDto, FieldFormModel, GeolocationFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-geolocation-validation',
    styleUrls: ['geolocation-validation.component.scss'],
    templateUrl: 'geolocation-validation.component.html',
})
export class GeolocationValidationComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<GeolocationFieldPropertiesDto>();
}
