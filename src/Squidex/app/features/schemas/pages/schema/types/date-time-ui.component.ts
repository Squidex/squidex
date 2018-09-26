/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Observable } from 'rxjs';

import { FieldDto, FloatConverter, NumberFieldPropertiesDto } from '@app/shared';

@Component({
    selector: 'sqx-date-time-ui',
    styleUrls: ['date-time-ui.component.scss'],
    templateUrl: 'date-time-ui.component.html'
})
export class DateTimeUIComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: NumberFieldPropertiesDto;

    public converter = new FloatConverter();

    public hideAllowedValues: Observable<boolean>;

    public ngOnInit() {
        this.editForm.setControl('editor',
            new FormControl(this.properties.editor, [
                Validators.required
            ]));
    }
}