/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';

import {
    ArrayFieldPropertiesDto,
    FieldDto,
    SchemaTagConverter
} from '@app/shared';

@Component({
    selector: 'sqx-array-validation',
    styleUrls: ['array-validation.component.scss'],
    templateUrl: 'array-validation.component.html'
})
export class ArrayValidationComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: ArrayFieldPropertiesDto;

    constructor(
        public readonly schemasSource: SchemaTagConverter
    ) {
    }

    public ngOnInit() {
        this.editForm.setControl('maxItems',
            new FormControl(this.properties.maxItems));

        this.editForm.setControl('minItems',
            new FormControl(this.properties.minItems));
    }
}