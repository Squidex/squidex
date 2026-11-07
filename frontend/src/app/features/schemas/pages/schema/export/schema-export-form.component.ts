/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, effect, inject, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { CodeEditorComponent, SchemaDto, SchemasState, SynchronizeSchemaForm, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-schema-export-form',
    styleUrls: ['./schema-export-form.component.scss'],
    templateUrl: './schema-export-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        FormField,
        FormsModule,
        TranslatePipe,
    ],
})
export class SchemaExportFormComponent {
    private readonly schemasState = inject(SchemasState);

    /** The schema to export and synchronize. */
    public readonly schema = input.required<SchemaDto>();

    protected readonly synchronizeForm = new SynchronizeSchemaForm();

    protected readonly isEditable = computed(() => {
        return this.schema().canUpdateScripts;
    });

    constructor() {
        effect(() => {
            this.synchronizeForm.loadSchema(this.schema());
        });
    }

    protected synchronize() {
        if (!this.isEditable()) {
            return;
        }

        this.synchronizeForm.submit(value => {
            return this.schemasState.synchronize(this.schema(), value);
        });
    }
}
