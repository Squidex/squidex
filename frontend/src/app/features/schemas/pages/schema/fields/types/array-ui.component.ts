/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FieldTree, FormField } from '@angular/forms/signals';
import { ArrayFieldFormModel, ArrayFieldPropertiesDto, FieldDto, FieldFormModel, FormRowComponent, SignalSelectDirective } from '@app/shared';

const CALCULATED_DEFAULT_VALUES: ReadonlyArray<string> = ['EmptyArray', 'Null'];

@Component({
    selector: 'sqx-array-ui',
    styleUrls: ['array-ui.component.scss'],
    templateUrl: 'array-ui.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormField,
        FormRowComponent,
        FormsModule,
        SignalSelectDirective,
    ],
})
export class ArrayUIComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<ArrayFieldPropertiesDto>();

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<ArrayFieldFormModel>;
    });

    protected readonly calculatedDefaultValues = CALCULATED_DEFAULT_VALUES;
}
