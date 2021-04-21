/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { FieldDto, GeolocationFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-geolocation-ui',
    styleUrls: ['geolocation-ui.component.scss'],
    templateUrl: 'geolocation-ui.component.html'
})
export class GeolocationUIComponent implements OnChanges {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: GeolocationFieldPropertiesDto;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['fieldForm']) {
            this.fieldForm.setControl('editor',
                new FormControl(undefined, Validators.required));
        }

        this.fieldForm.patchValue(this.field.properties);
    }
}