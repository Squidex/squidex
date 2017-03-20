/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Observable } from 'rxjs';

import { FloatConverter, NumberFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-date-time-ui',
    styleUrls: ['date-time-ui.component.scss'],
    templateUrl: 'date-time-ui.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class DateTimeUIComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: NumberFieldPropertiesDto;

    public converter = new FloatConverter();

    public hideAllowedValues: Observable<boolean>;

    public ngOnInit() {
        this.editForm.addControl('editor',
            new FormControl(this.properties.editor, [
                Validators.required
            ]));
        this.editForm.addControl('placeholder',
            new FormControl(this.properties.placeholder, [
                Validators.maxLength(100)
            ]));
    }
}