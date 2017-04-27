/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

import { DateTimeFieldPropertiesDto, ValidatorsEx } from 'shared';

@Component({
    selector: 'sqx-date-time-validation',
    styleUrls: ['date-time-validation.component.scss'],
    templateUrl: 'date-time-validation.component.html'
})
export class DateTimeValidationComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: DateTimeFieldPropertiesDto;

    public hideDefaultValues: Observable<boolean>;
    public hideDefaultValue: Observable<boolean>;

    public calculatedDefaultValues = ['Now', 'Today'];

    public ngOnInit() {
        this.editForm.addControl('calculatedDefaultValue',
            new FormControl(this.properties.calculatedDefaultValue));

        this.editForm.addControl('maxValue',
            new FormControl(this.properties.maxValue, [
                ValidatorsEx.validDateTime()
            ]));

        this.editForm.addControl('minValue',
            new FormControl(this.properties.minValue, [
                ValidatorsEx.validDateTime()
            ]));

        this.editForm.addControl('defaultValue',
            new FormControl(this.properties.defaultValue, [
                ValidatorsEx.validDateTime()
            ]));

        this.hideDefaultValues =
            this.editForm.get('isRequired')!.valueChanges
                .startWith(this.properties.isRequired)
                .map(x => !!x);

        this.hideDefaultValue =
            this.editForm.get('calculatedDefaultValue')!.valueChanges
                .startWith(this.properties.calculatedDefaultValue)
                .map(x => !!x);
    }
}