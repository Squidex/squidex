/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { DateTimeFieldPropertiesDto, DATETIME_FIELD_EDITORS, FieldDto, FloatConverter } from '@app/shared';

@Component({
    selector: 'sqx-date-time-ui',
    styleUrls: ['date-time-ui.component.scss'],
    templateUrl: 'date-time-ui.component.html',
})
export class DateTimeUIComponent {
    public readonly converter = FloatConverter.INSTANCE;

    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: DateTimeFieldPropertiesDto;

    public editors = DATETIME_FIELD_EDITORS;
}
