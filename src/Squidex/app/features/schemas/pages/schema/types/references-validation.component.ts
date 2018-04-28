/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

import { FieldDto, ReferencesFieldPropertiesDto, SchemasState } from '@app/shared';

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
        public readonly schemasState: SchemasState
    ) {
    }

    public ngOnInit() {
        this.editForm.setControl('maxItems',
            new FormControl(this.properties.maxItems));

        this.editForm.setControl('minItems',
            new FormControl(this.properties.minItems));

        this.editForm.setControl('schemaId',
            new FormControl(this.properties.schemaId, [
                Validators.required
            ]));
    }
}