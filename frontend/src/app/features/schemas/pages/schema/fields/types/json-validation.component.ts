/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { FieldDto, JsonFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-json-validation',
    styleUrls: ['json-validation.component.scss'],
    templateUrl: 'json-validation.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class JsonValidationComponent {
    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The type specific properties of the field. */
    public readonly properties = input.required<JsonFieldPropertiesDto>();
}
