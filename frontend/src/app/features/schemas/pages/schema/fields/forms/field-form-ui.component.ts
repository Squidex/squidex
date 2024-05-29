/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { FieldDto, FormHintComponent, SchemaDto, TranslatePipe } from '@app/shared';
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

@Component({
    standalone: true,
    selector: 'sqx-field-form-ui',
    styleUrls: ['./field-form-ui.component.scss'],
    templateUrl: './field-form-ui.component.html',
    imports: [
        ArrayUIComponent,
        AssetsUIComponent,
        BooleanUIComponent,
        ComponentUIComponent,
        ComponentsUIComponent,
        DateTimeUIComponent,
        FormHintComponent,
        FormsModule,
        GeolocationUIComponent,
        JsonUIComponent,
        NumberUIComponent,
        ReactiveFormsModule,
        RichTextUIComponent,
        ReferencesUIComponent,
        StringUIComponent,
        TagsUIComponent,
        TranslatePipe,
    ],
})
export class FieldFormUIComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public schema!: SchemaDto;
}
