/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { DateTimeFieldPropertiesDto, FieldDto, hasNoValue$, LanguageDto, TypedSimpleChanges } from '@app/shared';

const CALCULATED_DEFAULT_VALUES: ReadonlyArray<string> = ['Now', 'Today'];

@Component({
    selector: 'sqx-date-time-validation[field][fieldForm][languages][properties]',
    styleUrls: ['date-time-validation.component.scss'],
    templateUrl: 'date-time-validation.component.html',
})
export class DateTimeValidationComponent {
    @Input()
    public fieldForm!: UntypedFormGroup;

    @Input()
    public field!: FieldDto;

    @Input()
    public properties!: DateTimeFieldPropertiesDto;

    @Input()
    public languages!: ReadonlyArray<LanguageDto>;

    @Input()
    public isLocalizable?: boolean | null;

    public showDefaultValues?: Observable<boolean>;
    public showDefaultValue?: Observable<boolean>;

    public calculatedDefaultValues = CALCULATED_DEFAULT_VALUES;

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.fieldForm) {
            this.showDefaultValues =
                hasNoValue$(this.fieldForm.controls['isRequired']);

            this.showDefaultValue =
                hasNoValue$(this.fieldForm.controls['calculatedDefaultValue']);
        }
    }
}
