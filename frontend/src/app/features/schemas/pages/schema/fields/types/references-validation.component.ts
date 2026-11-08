/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, computed, inject, input } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { AppLanguageDto, FieldDto, FieldFormModel, FormRowComponent, LocalizedInputComponent, ReferencesFieldFormModel, ReferencesFieldPropertiesDto, SchemaTagSource, TagEditorComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-references-validation',
    styleUrls: ['references-validation.component.scss'],
    templateUrl: 'references-validation.component.html',
    imports: [
        FormField,
        FormRowComponent,
        LocalizedInputComponent,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class ReferencesValidationComponent {
    protected readonly schemasSource = inject(SchemaTagSource);

    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<ReferencesFieldPropertiesDto>();

    /** The languages of the app, used to edit localized default values. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Shows the localized default values, if the field is localizable. */
    public readonly isLocalizable = input(false, { transform: booleanAttribute });

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<ReferencesFieldFormModel>;
    });
}
