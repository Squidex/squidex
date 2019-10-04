/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

import {
    FieldDto,
    ReferencesFieldPropertiesDto,
    SchemaTagConverter
} from '@app/shared';

@Component({
    selector: 'sqx-references-validation',
    styleUrls: ['references-validation.component.scss'],
    templateUrl: 'references-validation.component.html'
})
export class ReferencesValidationComponent implements OnInit {
    @Input()
    public editForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: ReferencesFieldPropertiesDto;

    constructor(
        public readonly schemasSource: SchemaTagConverter
    ) {
    }

    public ngOnInit() {
        this.editForm.setControl('allowDuplicates',
            new FormControl(this.properties.allowDuplicates));

        this.editForm.setControl('maxItems',
            new FormControl(this.properties.maxItems));

        this.editForm.setControl('minItems',
            new FormControl(this.properties.minItems));

        this.editForm.setControl('schemaIds',
            new FormControl(this.properties.schemaIds, [
                Validators.required
            ]));
    }
}