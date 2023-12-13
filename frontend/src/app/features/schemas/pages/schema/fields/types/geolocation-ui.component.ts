/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { FieldDto, GeolocationFieldPropertiesDto, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-geolocation-ui',
    styleUrls: ['geolocation-ui.component.scss'],
    templateUrl: 'geolocation-ui.component.html',
    imports: [
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class GeolocationUIComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: GeolocationFieldPropertiesDto;
}
