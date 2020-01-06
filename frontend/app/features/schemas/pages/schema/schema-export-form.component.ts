/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    SchemaDetailsDto,
    SchemasState,
    SynchronizeSchemaForm
} from '@app/shared';

@Component({
    selector: 'sqx-schema-export-form',
    styleUrls: ['./schema-export-form.component.scss'],
    templateUrl: './schema-export-form.component.html'
})
export class SchemaExportFormComponent implements OnChanges {
    @Input()
    public schema: SchemaDetailsDto;

    public synchronizeForm = new SynchronizeSchemaForm(this.formBuilder);

    public isEditable = false;

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnChanges() {
        this.isEditable = this.schema.canUpdateScripts;

        this.synchronizeForm.loadSchema(this.schema);
    }

    public synchronize() {
        if (!this.isEditable) {
            return;
        }

        const value = this.synchronizeForm.submit();

        if (value) {
            this.schemasState.synchronize(this.schema, value)
                .subscribe(() => {
                    this.synchronizeForm.submitCompleted({ noReset: true });
                }, error => {
                    this.synchronizeForm.submitFailed(error);
                });
        }
    }
}