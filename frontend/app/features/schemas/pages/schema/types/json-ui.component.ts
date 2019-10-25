/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { FieldDto, JsonFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-json-ui',
    styleUrls: ['json-ui.component.scss'],
    templateUrl: 'json-ui.component.html'
})
export class JsonUIComponent {
    @Input()
    public editForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: JsonFieldPropertiesDto;
}