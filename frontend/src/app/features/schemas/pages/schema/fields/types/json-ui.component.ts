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
    standalone: true,
    selector: 'sqx-json-ui',
    styleUrls: ['json-ui.component.scss'],
    templateUrl: 'json-ui.component.html',
})
export class JsonUIComponent {
    @Input({ required: true })
    public fieldForm!: UntypedFormGroup;

    @Input({ required: true })
    public field!: FieldDto;

    @Input({ required: true })
    public properties!: JsonFieldPropertiesDto;
}
