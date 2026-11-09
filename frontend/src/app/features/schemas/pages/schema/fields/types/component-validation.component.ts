/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, inject, input } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { ComponentFieldFormModel, FieldDto, FieldFormModel, FormRowComponent, ReferencesFieldPropertiesDto, SchemaTagSource, TagEditorComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-component-validation',
    styleUrls: ['component-validation.component.scss'],
    templateUrl: 'component-validation.component.html',
    imports: [
        FormField,
        FormRowComponent,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class ComponentValidationComponent {
    protected readonly schemasSource = inject(SchemaTagSource);

    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<ReferencesFieldPropertiesDto>();

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<ComponentFieldFormModel>;
    });
}
