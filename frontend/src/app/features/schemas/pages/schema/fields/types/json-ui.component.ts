/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, input } from '@angular/core';
import { FieldTree } from '@angular/forms/signals';
import { FieldDto, FieldFormModel, JsonFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-json-ui',
    styleUrls: ['json-ui.component.scss'],
    templateUrl: 'json-ui.component.html',
})
export class JsonUIComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<JsonFieldPropertiesDto>();
}
