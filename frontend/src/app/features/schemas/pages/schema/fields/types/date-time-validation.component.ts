/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { Observable } from 'rxjs';
import { DateTimeEditorComponent, DateTimeFieldPropertiesDto, FieldDto, FormHintComponent, hasNoValue$, LanguageDto, LocalizedInputComponent, TranslatePipe, TypedSimpleChanges } from '@app/shared';

const CALCULATED_DEFAULT_VALUES: ReadonlyArray<string> = ['Now', 'Today'];

@Component({
    standalone: true,
    selector: 'sqx-date-time-validation',
    styleUrls: ['date-time-validation.component.scss'],
    templateUrl: 'date-time-validation.component.html',
    imports: [
        AsyncPipe,
        DateTimeEditorComponent,
        FormHintComponent,
        FormsModule,
        LocalizedInputComponent,
        ReactiveFormsModule,
        TranslatePipe,
    ],
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
