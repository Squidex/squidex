/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, input } from '@angular/core';
import { FieldTree } from '@angular/forms/signals';
import { FieldDto, FieldFormModel, ReferencesFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-component-ui',
    styleUrls: ['component-ui.component.scss'],
    templateUrl: 'component-ui.component.html',
})
export class ComponentUIComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<ReferencesFieldPropertiesDto>();
}
