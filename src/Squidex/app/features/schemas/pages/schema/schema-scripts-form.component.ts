/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import { SchemaDetailsDto, UpdateSchemaScriptsDto } from 'shared';

import { SchemasState } from './../../state/schemas.state';

@Component({
    selector: 'sqx-schema-scripts-form',
    styleUrls: ['./schema-scripts-form.component.scss'],
    templateUrl: './schema-scripts-form.component.html'
})
export class SchemaScriptsFormComponent implements OnInit {
    @Output()
    public completed = new EventEmitter();

    @Input()
    public schema: SchemaDetailsDto;

    public selectedField = 'scriptQuery';

    public scripts = [
        'Query',
        'Create',
        'Update',
        'Delete',
        'Change'
    ];

    public editFormSubmitted = false;
    public editForm =
        this.formBuilder.group({
            scriptQuery: '',
            scriptCreate: '',
            scriptUpdate: '',
            scriptDelete: '',
            scriptChange: ''
        });

    constructor(
        private readonly schemasState: SchemasState,
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.editForm.patchValue(this.schema);
    }

    public complete() {
        this.completed.emit();
    }

    public saveSchema() {
        this.editFormSubmitted = true;

        if (this.editForm.valid) {
            this.editForm.disable();

            const requestDto = <UpdateSchemaScriptsDto>this.editForm.value;

            this.schemasState.configureScripts(this.schema, requestDto)
                .subscribe(dto => {
                    this.complete();
                }, error => {
                    this.enableEditForm();
                });
        }
    }

    public selectField(field: string) {
        this.selectedField = field;
    }

    private enableEditForm() {
        this.editForm.enable();
        this.editFormSubmitted = false;
    }
}