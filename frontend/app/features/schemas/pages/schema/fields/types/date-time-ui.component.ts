/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { FieldDto, FloatConverter, NumberFieldPropertiesDto } from '@app/shared';
import { Observable } from 'rxjs';

@Component({
    selector: 'sqx-date-time-ui',
    styleUrls: ['date-time-ui.component.scss'],
    templateUrl: 'date-time-ui.component.html'
})
export class DateTimeUIComponent implements OnInit {
    public readonly converter = FloatConverter.INSTANCE;

    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: NumberFieldPropertiesDto;

    public hideAllowedValues: Observable<boolean>;

    public ngOnInit() {
        this.fieldForm.setControl('editor',
            new FormControl(this.properties.editor, [
                Validators.required
            ]));
    }
}