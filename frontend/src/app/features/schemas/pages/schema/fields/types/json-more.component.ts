/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormsModule, ReactiveFormsModule, UntypedFormGroup } from '@angular/forms';
import { CodeEditorComponent, FieldDto, FormRowComponent, JsonFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-json-more',
    styleUrls: ['json-more.component.scss'],
    templateUrl: 'json-more.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        FormRowComponent,
        FormsModule,
        ReactiveFormsModule,
    ],
})
export class JsonMoreComponent {
    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<JsonFieldPropertiesDto>();
}
