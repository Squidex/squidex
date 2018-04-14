/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    ApiUrlConfig,
    AppsState,
    CreateSchemaForm,
    SchemasState,
    SchemaDto
} from '@app/shared';

@Component({
    selector: 'sqx-schema-form',
    styleUrls: ['./schema-form.component.scss'],
    templateUrl: './schema-form.component.html'
})
export class SchemaFormComponent implements OnInit {
    @Output()
    public created = new EventEmitter<SchemaDto>();

    @Output()
    public cancelled = new EventEmitter();

    @Input()
    public import: any;

    public createForm: CreateSchemaForm;

    public showImport = false;

    constructor(formBuilder: FormBuilder,
        public readonly apiUrl: ApiUrlConfig,
        public readonly appsState: AppsState,
        private readonly schemasState: SchemasState
    ) {
        this.createForm = new CreateSchemaForm(formBuilder);
    }

    public ngOnInit() {
        this.createForm.load({ import: this.import });

        this.showImport = !!this.import;
    }

    public toggleImport() {
        this.showImport = !this.showImport;

        return false;
    }

    public complete(schema: SchemaDto) {
        this.created.emit(schema);
    }

    public cancel() {
        this.cancelled.emit();
    }

    public createSchema() {
        const value = this.createForm.submit();

        if (value) {
            const schemaName = value.name;
            const schemaDto = Object.assign(value.import || {}, { name: schemaName });

            this.schemasState.create(schemaDto)
                .subscribe(dto => {
                    this.complete(dto);
                }, error => {
                    this.createForm.submitFailed(error);
                });
        }
    }
}