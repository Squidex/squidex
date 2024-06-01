/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CodeEditorComponent, SchemaDto, SchemasState, SynchronizeSchemaForm, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-schema-export-form',
    styleUrls: ['./schema-export-form.component.scss'],
    templateUrl: './schema-export-form.component.html',
    imports: [
        CodeEditorComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class SchemaExportFormComponent {
    @Input({ required: true })
    public schema!: SchemaDto;

    public synchronizeForm = new SynchronizeSchemaForm();

    public isEditable = false;

    constructor(
        private readonly schemasState: SchemasState,
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
                .subscribe({
                    next: () => {
                        this.synchronizeForm.submitCompleted({ noReset: true });
                    },
                    error: error => {
                        this.synchronizeForm.submitFailed(error);
                    },
                });
        }
    }
}
