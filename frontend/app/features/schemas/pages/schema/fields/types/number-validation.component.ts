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
    FieldDto,
    hasNoValue$,
    NumberFieldPropertiesDto,
    RootFieldDto,
    Types
} from '@app/shared';

@Component({
    selector: 'sqx-number-validation',
    styleUrls: ['number-validation.component.scss'],
    templateUrl: 'number-validation.component.html'
})
export class NumberValidationComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: NumberFieldPropertiesDto;

    public showUnique: boolean;

    public showDefaultValue: Observable<boolean>;

    public ngOnInit() {
        this.showUnique = Types.is(this.field, RootFieldDto) && !this.field.isLocalizable;

        if (this.showUnique) {
            this.editForm.setControl('isUnique',
                new FormControl(this.properties.isUnique));
        }

        this.editForm.setControl('maxValue',
            new FormControl(this.properties.maxValue));

        this.editForm.setControl('minValue',
            new FormControl(this.properties.minValue));

        this.editForm.setControl('defaultValue',
            new FormControl(this.properties.defaultValue));

        this.showDefaultValue =
            hasNoValue$(this.editForm.controls['isRequired']);
    }
}