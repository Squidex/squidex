/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
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

    public showDefaultValue: Observable<boolean>;

    public ngOnInit() {
        this.editForm.setControl('maxValue',
            new FormControl(this.properties.maxValue));

        this.editForm.setControl('minValue',
            new FormControl(this.properties.minValue));

        this.editForm.setControl('defaultValue',
            new FormControl(this.properties.defaultValue));

        this.showDefaultValue =
            this.editForm.controls['isRequired'].valueChanges
                .startWith(this.properties.isRequired)
                .map(x => !x);
    }
}