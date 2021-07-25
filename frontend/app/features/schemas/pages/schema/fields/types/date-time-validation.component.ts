/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { DateTimeFieldPropertiesDto, FieldDto, hasNoValue$, LanguageDto } from '@app/shared';
import { Observable } from 'rxjs';

const CALCULATED_DEFAULT_VALUES: ReadonlyArray<string> = ['Now', 'Today'];

@Component({
    selector: 'sqx-date-time-validation[field][fieldForm][properties]',
    styleUrls: ['date-time-validation.component.scss'],
    templateUrl: 'date-time-validation.component.html',
})
export class DateTimeValidationComponent implements OnChanges {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: DateTimeFieldPropertiesDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable?: boolean | null;

    public showDefaultValues: Observable<boolean>;
    public showDefaultValue: Observable<boolean>;

    public calculatedDefaultValues = CALCULATED_DEFAULT_VALUES;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['fieldForm']) {
            this.showDefaultValues =
                hasNoValue$(this.fieldForm.controls['isRequired']);

            this.showDefaultValue =
                hasNoValue$(this.fieldForm.controls['calculatedDefaultValue']);
        }
    }
}
