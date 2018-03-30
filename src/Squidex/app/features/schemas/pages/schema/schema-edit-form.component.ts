/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import { SchemaDetailsDto } from '@app/shared';
import { SchemasState, EditSchemaForm } from './../../state/schemas.state';

@Component({
    selector: 'sqx-schema-edit-form',
    styleUrls: ['./schema-edit-form.component.scss'],
    templateUrl: './schema-edit-form.component.html'
})
export class SchemaEditFormComponent implements OnInit {
    @Output()
    public completed = new EventEmitter();

    @Input()
    public schema: SchemaDetailsDto;

    public editForm: EditSchemaForm;

    constructor(formBuilder: FormBuilder,
        private readonly schemasState: SchemasState
    ) {
        this.editForm = new EditSchemaForm(formBuilder);
    }

    public ngOnInit() {
        this.editForm.load(this.schema.properties);
    }

    public complete() {
        this.completed.emit();
    }

    public saveSchema() {
        const value = this.editForm.submit();

        if (value) {
            this.schemasState.update(this.schema, value)
                .subscribe(dto => {
                    this.complete();
                }, error => {
                    this.editForm.submitFailed(error);
                });
        }
    }
}