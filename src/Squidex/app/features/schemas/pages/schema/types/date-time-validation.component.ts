/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

import {
    DateTimeFieldPropertiesDto,
    FieldDto,
    hasNoValue$,
    ValidatorsEx
} from '@app/shared';

@Component({
    selector: 'sqx-date-time-validation',
    styleUrls: ['date-time-validation.component.scss'],
    templateUrl: 'date-time-validation.component.html'
})
export class DateTimeValidationComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: DateTimeFieldPropertiesDto;

    public showDefaultValues: Observable<boolean>;
    public showDefaultValue: Observable<boolean>;

    public calculatedDefaultValues: ReadonlyArray<string> = ['Now', 'Today'];

    public ngOnInit() {
        this.editForm.setControl('calculatedDefaultValue',
            new FormControl(this.properties.calculatedDefaultValue));

        this.editForm.setControl('maxValue',
            new FormControl(this.properties.maxValue, [
                ValidatorsEx.validDateTime()
            ]));

        this.editForm.setControl('minValue',
            new FormControl(this.properties.minValue, [
                ValidatorsEx.validDateTime()
            ]));

        this.editForm.setControl('defaultValue',
            new FormControl(this.properties.defaultValue, [
                ValidatorsEx.validDateTime()
            ]));

        this.showDefaultValues =
            hasNoValue$(this.editForm.controls['isRequired']);

        this.showDefaultValue =
            hasNoValue$(this.editForm.controls['calculatedDefaultValue']);
    }
}