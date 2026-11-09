/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, Component, computed, input } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { AppLanguageDto, BooleanFieldFormModel, BooleanFieldPropertiesDto, CheckboxComponent, FieldDto, FieldFormModel, FormRowComponent, LocalizedInputComponent } from '@app/shared';

@Component({
    selector: 'sqx-boolean-validation',
    styleUrls: ['boolean-validation.component.scss'],
    templateUrl: 'boolean-validation.component.html',
    imports: [
        CheckboxComponent,
        FormField,
        FormRowComponent,
        LocalizedInputComponent,
    ],
})
export class BooleanValidationComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<BooleanFieldPropertiesDto>();

    /** The languages of the app, used to edit localized default values. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Shows the localized default values, if the field is localizable. */
    public readonly isLocalizable = input(false, { transform: booleanAttribute });

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<BooleanFieldFormModel>;
    });
}
