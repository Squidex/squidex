/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, input } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { switchMap } from 'rxjs/operators';
import { AppLanguageDto, DateTimeEditorComponent, DateTimeFieldPropertiesDto, FieldDto, FormRowComponent, hasNoValue$, LocalizedInputComponent, valueProjection$ } from '@app/shared';

const CALCULATED_DEFAULT_VALUES: ReadonlyArray<string> = ['Now', 'Today'];

@Component({
    selector: 'sqx-date-time-validation',
    styleUrls: ['date-time-validation.component.scss'],
    templateUrl: 'date-time-validation.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DateTimeEditorComponent,
        FormRowComponent,
        FormsModule,
        LocalizedInputComponent,
        ReactiveFormsModule,
    ],
})
export class DateTimeValidationComponent {
    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<DateTimeFieldPropertiesDto>();

    /** The languages of the app, used to edit localized default values. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Shows the localized default values, if the field is localizable. */
    public readonly isLocalizable = input(false, { transform: booleanAttribute });

    protected readonly showDefaultValues = toSignal(
        toObservable(this.fieldForm).pipe(
            switchMap(form => valueProjection$(form.controls['isRequired'], x => x !== true))),
        { initialValue: false });

    protected readonly showDefaultValue = toSignal(
        toObservable(this.fieldForm).pipe(
            switchMap(form => hasNoValue$(form.controls['calculatedDefaultValue']))),
        { initialValue: false });

    protected readonly calculatedDefaultValues = CALCULATED_DEFAULT_VALUES;
}
