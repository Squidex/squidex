/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { FieldDto, JsonFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-json-more',
    styleUrls: ['json-more.component.scss'],
    templateUrl: 'json-more.component.html',
})
export class JsonMoreComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: JsonFieldPropertiesDto;
}
