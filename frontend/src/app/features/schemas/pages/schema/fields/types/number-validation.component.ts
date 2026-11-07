/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FieldTree, FormField } from '@angular/forms/signals';
import { AppLanguageDto, FieldDto, FieldFormModel, FormRowComponent, LocalizedInputComponent, NumberFieldFormModel, NumberFieldPropertiesDto, SchemaDto, TranslatePipe, Types } from '@app/shared';

@Component({
    selector: 'sqx-number-validation',
    styleUrls: ['number-validation.component.scss'],
    templateUrl: 'number-validation.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormField,
        FormRowComponent,
        FormsModule,
        LocalizedInputComponent,
        TranslatePipe,
    ],
})
export class NumberValidationComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

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

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<NumberFieldFormModel>;
    });

    protected readonly showUnique = computed(() => {
        const field = this.field();

        return Types.is(field, FieldDto) && !field.isLocalizable && this.schema().type !== 'Component';
    });
}
