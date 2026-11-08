/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { BooleanFieldEditorValues, BooleanFieldFormModel, BooleanFieldPropertiesDto, FieldDto, FieldFormModel, FormRowComponent } from '@app/shared';

@Component({
    selector: 'sqx-boolean-ui',
    styleUrls: ['boolean-ui.component.scss'],
    templateUrl: 'boolean-ui.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormField,
        FormRowComponent,
    ],
})
export class BooleanUIComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<BooleanFieldPropertiesDto>();

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<BooleanFieldFormModel>;
    });

    protected readonly editors = BooleanFieldEditorValues;
}
