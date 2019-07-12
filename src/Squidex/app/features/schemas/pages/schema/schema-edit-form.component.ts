/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    EditSchemaForm,
    SchemaDetailsDto,
    SchemasState
} from '@app/shared';

@Component({
    selector: 'sqx-schema-edit-form',
    styleUrls: ['./schema-edit-form.component.scss'],
    templateUrl: './schema-edit-form.component.html'
})
export class SchemaEditFormComponent implements OnInit {
    @Output()
    public complete = new EventEmitter();

    @Input()
    public schema: SchemaDetailsDto;

    public editForm = new EditSchemaForm(this.formBuilder);

    public isEditable = false;

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnInit() {
        this.isEditable = this.schema.canUpdate;

        this.editForm.load(this.schema.properties);
        this.editForm.setEnabled(this.isEditable);
    }

    public emitComplete() {
        this.complete.emit();
    }

    public saveSchema() {
        if (!this.isEditable) {
            return;
        }

        const value = this.editForm.submit();

        if (value) {
            this.schemasState.update(this.schema, value)
                .subscribe(() => {
                    this.emitComplete();
                }, error => {
                    this.editForm.submitFailed(error);
                });
        }
    }
}