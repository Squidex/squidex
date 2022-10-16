/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { BOOLEAN_FIELD_EDITORS, BooleanFieldPropertiesDto, FieldDto } from '@app/shared';

@Component({
    selector: 'sqx-boolean-ui[field][fieldForm][properties]',
    styleUrls: ['boolean-ui.component.scss'],
    templateUrl: 'boolean-ui.component.html',
})
export class BooleanUIComponent {
    public readonly editors = BOOLEAN_FIELD_EDITORS;

    @Input()
    public fieldForm!: UntypedFormGroup;

    @Input()
    public field!: FieldDto;

    @Input()
    public properties!: BooleanFieldPropertiesDto;
}
