/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppLanguageDto, EditSchemaForm, FormRowComponent, SchemaDto, SchemasState, TagEditorComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-schema-edit-form',
    styleUrls: ['./schema-edit-form.component.scss'],
    templateUrl: './schema-edit-form.component.html',
    imports: [
        FormRowComponent,
        FormsModule,
        ReactiveFormsModule,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class SchemaEditFormComponent {
    @Input({ required: true })
    public schema!: SchemaDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    public isEditable?: boolean | null;

    public schemaForm = new EditSchemaForm();

    public fieldNames: ReadonlyArray<string> = [];

    constructor(
        private readonly schemasState: SchemasState,
    ) {
    }

    public ngOnChanges() {
        this.isEditable = this.schema.canUpdate;

        this.schemaForm.load(this.schema.properties);
        this.schemaForm.setEnabled(this.isEditable);

        const fieldNames = new Set<string>();
        for (const field of this.schema.fields) {
            if (field.properties.isContentField && field.properties.fieldType === 'String') {
                if (field.partitioning === 'invariant') {
                    fieldNames.add(`data.${field.name}.iv`);
                } else {
                    for (const language of this.languages) {
                        fieldNames.add(`data.${field.name}.${language}`);
                    }
                }
            }
        }

        this.fieldNames = [...fieldNames].sorted();
    }

    public saveSchema() {
        if (!this.isEditable) {
            return;
        }

        const value = this.schemaForm.submit();
        if (!value) {
            return;
        }

        this.schemasState.update(this.schema, value)
            .subscribe({
                next: () => {
                    this.schemaForm.submitCompleted({ noReset: true });
                },
                error: error => {
                    this.schemaForm.submitFailed(error);
                },
            });
    }
}
