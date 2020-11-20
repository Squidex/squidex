/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { DateTimeFieldPropertiesDto, FieldDto, hasNoValue$, LanguageDto, ValidatorsEx } from '@app/shared';
import { Observable } from 'rxjs';

@Component({
    selector: 'sqx-date-time-validation',
    styleUrls: ['date-time-validation.component.scss'],
    templateUrl: 'date-time-validation.component.html'
})
export class DateTimeValidationComponent implements OnInit {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: DateTimeFieldPropertiesDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable: boolean;

    public showDefaultValues: Observable<boolean>;
    public showDefaultValue: Observable<boolean>;

    public calculatedDefaultValues: ReadonlyArray<string> = ['Now', 'Today'];

    public ngOnInit() {
        this.fieldForm.setControl('calculatedDefaultValue',
            new FormControl(this.properties.calculatedDefaultValue));

        this.fieldForm.setControl('maxValue',
            new FormControl(this.properties.maxValue, [
                ValidatorsEx.validDateTime()
            ]));

        this.fieldForm.setControl('minValue',
            new FormControl(this.properties.minValue, [
                ValidatorsEx.validDateTime()
            ]));

        this.fieldForm.setControl('defaultValue',
            new FormControl(this.properties.defaultValue));

        this.fieldForm.setControl('defaultValues',
            new FormControl(this.properties.defaultValues));

        this.showDefaultValues =
            hasNoValue$(this.fieldForm.controls['isRequired']);

        this.showDefaultValue =
            hasNoValue$(this.fieldForm.controls['calculatedDefaultValue']);
    }
}