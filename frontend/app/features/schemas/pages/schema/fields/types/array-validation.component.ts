/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { ArrayFieldPropertiesDto, FieldDto, SchemaTagSource } from '@app/shared';

@Component({
    selector: 'sqx-array-validation',
    styleUrls: ['array-validation.component.scss'],
    templateUrl: 'array-validation.component.html'
})
export class ArrayValidationComponent implements OnInit {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: ArrayFieldPropertiesDto;

    constructor(
        public readonly schemasSource: SchemaTagSource
    ) {
    }

    public ngOnInit() {
        this.fieldForm.setControl('maxItems',
            new FormControl(this.properties.maxItems));

        this.fieldForm.setControl('minItems',
            new FormControl(this.properties.minItems));
    }
}