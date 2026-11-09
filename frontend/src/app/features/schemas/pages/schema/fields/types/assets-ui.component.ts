/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FieldTree, FormField } from '@angular/forms/signals';
import { AssetFolderDropdownComponent, AssetsFieldFormModel, AssetsFieldPropertiesDto, FieldDto, FieldFormModel, FormRowComponent, SignalSelectDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-assets-ui',
    styleUrls: ['assets-ui.component.scss'],
    templateUrl: 'assets-ui.component.html',
    imports: [
        AssetFolderDropdownComponent,
        FormField,
        FormRowComponent,
        FormsModule,
        SignalSelectDirective,
        TranslatePipe,
    ],
})
export class AssetsUIComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<AssetsFieldPropertiesDto>();

    protected readonly form = computed(() => {
        return this.fieldForm() as FieldTree<AssetsFieldFormModel>;
    });
}
