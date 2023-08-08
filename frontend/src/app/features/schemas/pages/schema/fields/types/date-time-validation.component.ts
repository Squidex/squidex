/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { DateTimeFieldPropertiesDto, FieldDto, hasNoValue$, LanguageDto, TypedSimpleChanges } from '@app/shared';

const CALCULATED_DEFAULT_VALUES: ReadonlyArray<string> = ['Now', 'Today'];

@Component({
    selector: 'sqx-date-time-validation',
    styleUrls: ['date-time-validation.component.scss'],
    templateUrl: 'date-time-validation.component.html',
})
export class DateTimeValidationComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: DateTimeFieldPropertiesDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<LanguageDto>;

    @Input({ transform: booleanAttribute })
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
