/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, input } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { FieldDto, FieldFormModel, FormRowComponent, SchemaDto } from '@app/shared';
import { ArrayUIComponent } from '../types/array-ui.component';
import { AssetsUIComponent } from '../types/assets-ui.component';
import { BooleanUIComponent } from '../types/boolean-ui.component';
import { ComponentUIComponent } from '../types/component-ui.component';
import { ComponentsUIComponent } from '../types/components-ui.component';
import { DateTimeUIComponent } from '../types/date-time-ui.component';
import { GeolocationUIComponent } from '../types/geolocation-ui.component';
import { JsonUIComponent } from '../types/json-ui.component';
import { NumberUIComponent } from '../types/number-ui.component';
import { ReferencesUIComponent } from '../types/references-ui.component';
import { RichTextUIComponent } from '../types/rich-text-ui.component';
import { StringUIComponent } from '../types/string-ui.component';
import { TagsUIComponent } from '../types/tags-ui.component';
import { UserInfoUIComponent } from '../types/user-info-ui.component';

@Component({
    selector: 'sqx-field-form-ui',
    styleUrls: ['./field-form-ui.component.scss'],
    templateUrl: './field-form-ui.component.html',
    imports: [
        ArrayUIComponent,
        AssetsUIComponent,
        BooleanUIComponent,
        ComponentsUIComponent,
        ComponentUIComponent,
        DateTimeUIComponent,
        FormField,
        FormRowComponent,
        GeolocationUIComponent,
        JsonUIComponent,
        NumberUIComponent,
        ReferencesUIComponent,
        RichTextUIComponent,
        StringUIComponent,
        TagsUIComponent,
        UserInfoUIComponent,
    ],
})
export class FieldFormUIComponent {
    /** The form to edit the field properties. */
    public readonly fieldForm = input.required<FieldTree<FieldFormModel>>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The schema the field belongs to. */
    public readonly schema = input.required<SchemaDto>();
}
