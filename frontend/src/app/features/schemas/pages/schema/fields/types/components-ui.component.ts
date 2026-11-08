/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, computed, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FieldTree, FormField } from '@angular/forms/signals';
import { ComponentsFieldFormModel, FieldDto, FieldFormModel, FormRowComponent, ReferencesFieldPropertiesDto, SignalSelectDirective } from '@app/shared';

const CALCULATED_DEFAULT_VALUES: ReadonlyArray<string> = ['EmptyArray', 'Null'];

@Component({
    selector: 'sqx-components-ui',
    styleUrls: ['components-ui.component.scss'],
    templateUrl: 'components-ui.component.html',
    imports: [
        FormField,
        FormRowComponent,
        FormsModule,
        SignalSelectDirective,
    ],
})
export class ComponentsUIComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<ReferencesFieldPropertiesDto>();

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<ComponentsFieldFormModel>;
    });

    protected readonly calculatedDefaultValues = CALCULATED_DEFAULT_VALUES;
}
