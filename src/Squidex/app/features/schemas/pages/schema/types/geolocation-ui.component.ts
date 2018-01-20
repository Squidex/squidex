/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';

import { GeolocationFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-geolocation-ui',
    styleUrls: ['geolocation-ui.component.scss'],
    templateUrl: 'geolocation-ui.component.html'
})
export class GeolocationUIComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: GeolocationFieldPropertiesDto;

    public ngOnInit() {
        this.editForm.addControl('editor',
            new FormControl(this.properties.editor, [
                Validators.required
            ]));
    }
}