/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

import {
    BooleanFieldPropertiesDto,
    FieldDto,
    hasNoValue$
} from '@app/shared';

@Component({
    selector: 'sqx-boolean-validation',
    styleUrls: ['boolean-validation.component.scss'],
    templateUrl: 'boolean-validation.component.html'
})
export class BooleanValidationComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: BooleanFieldPropertiesDto;

    public showDefaultValue: Observable<boolean>;

    public ngOnInit() {
        this.editForm.setControl('defaultValue',
            new FormControl(this.properties.defaultValue));

        this.editForm.setControl('inlineEditable',
            new FormControl(this.properties.inlineEditable));

        this.showDefaultValue =
            hasNoValue$(this.editForm.controls['isRequired']);
    }
}