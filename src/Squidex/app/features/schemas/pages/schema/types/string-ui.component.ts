/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

import { fadeAnimation, StringFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-string-ui',
    styleUrls: ['string-ui.component.scss'],
    templateUrl: 'string-ui.component.html',
    animations: [
        fadeAnimation
    ]
})
export class StringUIComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: StringFieldPropertiesDto;

    public ngOnInit() {
        this.editForm.addControl('placeholder',
            new FormControl('', [
                Validators.maxLength(100)
            ]));
    }
}