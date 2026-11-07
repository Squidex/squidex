/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FieldTree, FormField } from '@angular/forms/signals';
import { AppLanguageDto, DateTimeEditorComponent, DateTimeFieldFormModel, DateTimeFieldPropertiesDto, FieldDto, FieldFormModel, FormRowComponent, LocalizedInputComponent, SignalSelectDirective } from '@app/shared';

const CALCULATED_DEFAULT_VALUES: ReadonlyArray<string> = ['Now', 'Today'];

@Component({
    selector: 'sqx-date-time-validation',
    styleUrls: ['date-time-validation.component.scss'],
    templateUrl: 'date-time-validation.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DateTimeEditorComponent,
        FormField,
        FormRowComponent,
        FormsModule,
        LocalizedInputComponent,
        SignalSelectDirective,
    ],
})
export class DateTimeValidationComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<DateTimeFieldPropertiesDto>();

    /** The languages of the app, used to edit localized default values. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Shows the localized default values, if the field is localizable. */
    public readonly isLocalizable = input(false, { transform: booleanAttribute });

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<DateTimeFieldFormModel>;
    });

    protected readonly showDefaultValues = computed(() => {
        return !this.form().isRequired().value();
    });

    protected readonly showDefaultValue = computed(() => {
        return !this.form().calculatedDefaultValue().value();
    });

    protected readonly calculatedDefaultValues = CALCULATED_DEFAULT_VALUES;
}
