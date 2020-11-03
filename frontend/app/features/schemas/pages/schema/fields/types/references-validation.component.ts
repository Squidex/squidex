/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { FieldDto, ReferencesFieldPropertiesDto, SchemaTagSource } from '@app/shared';

@Component({
    selector: 'sqx-references-validation',
    styleUrls: ['references-validation.component.scss'],
    templateUrl: 'references-validation.component.html'
})
export class ReferencesValidationComponent implements OnInit {
    @Input()
    public fieldForm: FormGroup;

    @Input()
    public field: FieldDto;

    @Input()
    public properties: ReferencesFieldPropertiesDto;

    constructor(
        public readonly schemasSource: SchemaTagSource
    ) {
    }

    public ngOnInit() {
        this.fieldForm.setControl('allowDuplicates',
            new FormControl(this.properties.allowDuplicates));

        this.fieldForm.setControl('maxItems',
            new FormControl(this.properties.maxItems));

        this.fieldForm.setControl('minItems',
            new FormControl(this.properties.minItems));

        this.fieldForm.setControl('schemaIds',
            new FormControl(this.properties.schemaIds));

        this.fieldForm.setControl('defaultValue',
            new FormControl(this.properties.defaultValue));

        this.fieldForm.setControl('mustBePublished',
            new FormControl(this.properties.mustBePublished));
    }
}