/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { FieldDto, FieldFormModel, FormRowComponent, ReferencesFieldEditorValues, ReferencesFieldFormModel, ReferencesFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-references-ui',
    styleUrls: ['references-ui.component.scss'],
    templateUrl: 'references-ui.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormField,
        FormRowComponent,
    ],
})
export class ReferencesUIComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<ReferencesFieldPropertiesDto>();

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<ReferencesFieldFormModel>;
    });

    protected readonly editors = ReferencesFieldEditorValues;
}
