/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { AppsState, CodeEditorComponent, EditSchemaScriptsForm, SchemaDto, SchemasService, SchemasState, ScriptNamePipe, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-schema-scripts-form',
    styleUrls: ['./schema-scripts-form.component.scss'],
    templateUrl: './schema-scripts-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        FormField,
        FormsModule,
        ScriptNamePipe,
        TranslatePipe,
    ],
})
export class SchemaScriptsFormComponent {
    private readonly appsState = inject(AppsState);
    private readonly schemasService = inject(SchemasService);
    private readonly schemasState = inject(SchemasState);

    /** The schema to edit the scripts for. */
    public readonly schema = input.required<SchemaDto>();

    protected readonly schemaScript = signal('query');

    protected readonly isEditable = computed(() => {
        return this.schema().canUpdateScripts;
    });

    protected readonly schemaCompletions = rxResource({
        params: () => this.schema(),
        stream: ({ params }) => this.schemasService.getContentScriptsCompletion(this.appsState.appName, params.name),
    });

    protected readonly editForm = new EditSchemaScriptsForm();

    constructor() {
        effect(() => {
            const schema = this.schema();

            this.editForm.load(schema.scripts);
            this.editForm.setEnabled(schema.canUpdateScripts);
        });
    }

    protected selectField(field: string) {
        this.schemaScript.set(field);
    }

    protected saveSchema() {
        if (!this.isEditable()) {
            return;
        }

        this.editForm.submit(value => {
            return this.schemasState.configureScripts(this.schema(), value);
        });
    }
}
