/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { BooleanFieldPropertiesDto, BOOLEAN_FIELD_EDITORS, FieldDto } from '@app/shared';

@Component({
    selector: 'sqx-boolean-ui',
    styleUrls: ['boolean-ui.component.scss'],
    templateUrl: 'boolean-ui.component.html',
})
export class BooleanUIComponent {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: BooleanFieldPropertiesDto;

    public editors = BOOLEAN_FIELD_EDITORS;
}
