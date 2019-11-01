/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    DialogService,
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
    public readonly standalone = { standalone: true };

    @Input()
    public schema: SchemaDetailsDto;

    public editForm = new EditSchemaForm(this.formBuilder);

    public isEditable = false;

    constructor(
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnInit() {
        this.isEditable = this.schema.canUpdate;

        this.editForm.load(this.schema.properties);
        this.editForm.setEnabled(this.isEditable);
    }

    public saveSchema() {
        if (!this.isEditable) {
            return;
        }

        const value = this.editForm.submit();

        if (value) {
            this.schemasState.update(this.schema, value)
                .subscribe(() => {
                    this.dialogs.notifyInfo('Schema saved successfully.');

                    this.editForm.submitCompleted({ noReset: true });
                }, error => {
                    this.editForm.submitFailed(error);
                });
        }
    }
}