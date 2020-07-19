/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { EditSchemaForm, SchemaDetailsDto, SchemasState } from '@app/shared';

@Component({
    selector: 'sqx-schema-edit-form',
    styleUrls: ['./schema-edit-form.component.scss'],
    templateUrl: './schema-edit-form.component.html'
})
export class SchemaEditFormComponent implements OnChanges {
    public readonly standalone = { standalone: true };

    @Input()
    public schema: SchemaDetailsDto;

    public fieldForm = new EditSchemaForm(this.formBuilder);

    public isEditable = false;

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnChanges() {
        this.isEditable = this.schema.canUpdate;

        this.fieldForm.load(this.schema.properties);
        this.fieldForm.setEnabled(this.isEditable);
    }

    public saveSchema() {
        if (!this.isEditable) {
            return;
        }

        const value = this.fieldForm.submit();

        if (value) {
            this.schemasState.update(this.schema, value)
                .subscribe(() => {
                    this.fieldForm.submitCompleted({ noReset: true });
                }, error => {
                    this.fieldForm.submitFailed(error);
                });
        }
    }
}