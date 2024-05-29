/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ControlErrorsComponent, EditSchemaForm, FormAlertComponent, FormHintComponent, SchemaDto, SchemasState, TagEditorComponent, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-schema-edit-form',
    styleUrls: ['./schema-edit-form.component.scss'],
    templateUrl: './schema-edit-form.component.html',
    imports: [
        ControlErrorsComponent,
        FormAlertComponent,
        FormHintComponent,
        FormsModule,
        ReactiveFormsModule,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class SchemaEditFormComponent {
    @Input({ required: true })
    public schema!: SchemaDto;

    public fieldForm = new EditSchemaForm();

    public isEditable?: boolean | null;

    constructor(
        private readonly schemasState: SchemasState,
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
                .subscribe({
                    next: () => {
                        this.fieldForm.submitCompleted({ noReset: true });
                    },
                    error: error => {
                        this.fieldForm.submitFailed(error);
                    },
                });
        }
    }
}
