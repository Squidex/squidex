/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { DateTimeFieldPropertiesDto, DATETIME_FIELD_EDITORS, FieldDto, FloatConverter } from '@app/shared';
import { Observable } from 'rxjs';

@Component({
    selector: 'sqx-date-time-ui',
    styleUrls: ['date-time-ui.component.scss'],
    templateUrl: 'date-time-ui.component.html'
})
export class DateTimeUIComponent implements OnChanges {
    public readonly converter = FloatConverter.INSTANCE;

    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: DateTimeFieldPropertiesDto;

    public editors = DATETIME_FIELD_EDITORS;

    public hideAllowedValues: Observable<boolean>;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['fieldForm']) {
            this.fieldForm.setControl('editor',
                new FormControl(undefined, Validators.required));
        }

        this.fieldForm.patchValue(this.properties);
    }
}