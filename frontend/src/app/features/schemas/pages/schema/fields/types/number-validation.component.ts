/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { AppLanguageDto, FieldDto, FormRowComponent, LocalizedInputComponent, NumberFieldPropertiesDto, SchemaDto, TranslatePipe, Types } from '@app/shared';

@Component({
    selector: 'sqx-number-validation',
    styleUrls: ['number-validation.component.scss'],
    templateUrl: 'number-validation.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormRowComponent,
        FormsModule,
        LocalizedInputComponent,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class NumberValidationComponent {
    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The schema the field belongs to. */
    public readonly schema = input.required<SchemaDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<NumberFieldPropertiesDto>();

    /** The languages of the app, used to edit localized default values. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Shows the localized default values, if the field is localizable. */
    public readonly isLocalizable = input(false, { transform: booleanAttribute });

    protected readonly showUnique = computed(() => {
        const field = this.field();

        return Types.is(field, FieldDto) && !field.isLocalizable && this.schema().type !== 'Component';
    });
}
