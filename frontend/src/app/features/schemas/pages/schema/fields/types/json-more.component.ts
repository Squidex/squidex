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
    selector: 'sqx-json-more[field][fieldForm][properties]',
    styleUrls: ['json-more.component.scss'],
    templateUrl: 'json-more.component.html',
})
export class JsonMoreComponent {
    @Input()
    public fieldForm!: UntypedFormGroup;

    @Input()
    public field!: FieldDto;

    @Input()
    public properties!: JsonFieldPropertiesDto;
}
