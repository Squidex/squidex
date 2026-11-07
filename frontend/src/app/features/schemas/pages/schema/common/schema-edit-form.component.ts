/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, effect, inject, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { AppLanguageDto, EditSchemaForm, FormRowComponent, SchemaDto, SchemasState, TagEditorComponent, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-schema-edit-form',
    styleUrls: ['./schema-edit-form.component.scss'],
    templateUrl: './schema-edit-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormField,
        FormRowComponent,
        FormsModule,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class SchemaEditFormComponent {
    private readonly schemasState = inject(SchemasState);

    /** The schema to edit. */
    public readonly schema = input.required<SchemaDto>();

    /** The languages of the app, to provide the localized search fields. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    protected readonly isEditable = computed(() => {
        return this.schema().canUpdate;
    });

    protected readonly fieldNames = computed(() => {
        const languages = this.languages();

        const fieldNames = new Set<string>();
        for (const field of this.schema().fields) {
            if (field.properties.isContentField && field.properties.fieldType === 'String') {
                if (field.partitioning === 'invariant') {
                    fieldNames.add(`data.${field.name}.iv`);
                } else {
                    for (const language of languages) {
                        fieldNames.add(`data.${field.name}.${language}`);
                    }
                }
            }
        }

        return [...fieldNames].sorted();
    });

    protected readonly schemaForm = new EditSchemaForm();

    constructor() {
        effect(() => {
            const schema = this.schema();

            this.schemaForm.load(schema.properties);
            this.schemaForm.setEnabled(schema.canUpdate);
        });
    }

    protected saveSchema() {
        if (!this.isEditable()) {
            return;
        }

        this.schemaForm.submit(value => {
            return this.schemasState.update(this.schema(), value);
        });
    }
}
