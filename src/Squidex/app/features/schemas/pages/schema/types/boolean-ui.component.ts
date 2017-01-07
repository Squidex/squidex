/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Observable } from 'rxjs';

import {
    fadeAnimation,
    FloatConverter,
    BooleanFieldPropertiesDto
} from 'shared';

@Component({
    selector: 'sqx-boolean-ui',
    styleUrls: ['boolean-ui.component.scss'],
    templateUrl: 'boolean-ui.component.html',
    animations: [
        fadeAnimation
    ]
})
export class BooleanUIComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: BooleanFieldPropertiesDto;

    public converter = new FloatConverter();

    public hideAllowedValues: Observable<boolean>;

    public ngOnInit() {
        this.editForm.addControl('editor',
            new FormControl(this.properties.editor, [
                Validators.required
            ]));
        this.editForm.addControl('placeholder',
            new FormControl(this.properties.placeholder, [
                Validators.maxLength(100)
            ]));
    }
}