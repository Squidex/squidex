/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

import { NumberFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-number-validation',
    styleUrls: ['number-validation.component.scss'],
    templateUrl: 'number-validation.component.html'
})
export class NumberValidationComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: NumberFieldPropertiesDto;

    public hideDefaultValue: Observable<boolean>;

    public ngOnInit() {
        this.editForm.addControl('maxValue',
            new FormControl(this.properties.maxValue));

        this.editForm.addControl('minValue',
            new FormControl(this.properties.minValue));

        this.editForm.addControl('defaultValue',
            new FormControl(this.properties.defaultValue));

        this.hideDefaultValue =
            this.editForm.controls['isRequired'].valueChanges
                .startWith(this.properties.isRequired)
                .map(x => !!x);
    }
}