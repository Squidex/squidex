/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, input, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { switchMap } from 'rxjs';
import { AppsState, CodeEditorComponent, EditSchemaScriptsForm, KeysPipe, SchemaDto, SchemasService, SchemasState, ScriptNamePipe, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-schema-scripts-form',
    styleUrls: ['./schema-scripts-form.component.scss'],
    templateUrl: './schema-scripts-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        FormsModule,
        KeysPipe,
        ReactiveFormsModule,
        ScriptNamePipe,
        TranslatePipe,
    ],
})
export class SchemaScriptsFormComponent {
    /** The schema to edit the scripts for. */
    public readonly schema = input.required<SchemaDto>();

    protected readonly schemaScript = signal('query');
    protected readonly schemaCompletions;

    protected readonly editForm = new EditSchemaScriptsForm();

    protected readonly isEditable = computed(() => {
        return this.schema().canUpdateScripts;
    });

    constructor(appsState: AppsState, schemasService: SchemasService,
        private readonly schemasState: SchemasState,
    ) {
        this.schemaCompletions = toSignal(
            toObservable(this.schema).pipe(
                switchMap(schema => schemasService.getContentScriptsCompletion(appsState.appName, schema.name))));

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

        const value = this.editForm.submit();
        if (!value) {
            return;
        }

        this.schemasState.configureScripts(this.schema(), value)
            .subscribe({
                next: () => {
                    this.editForm.submitCompleted({ noReset: true });
                },
                error: error => {
                    this.editForm.submitFailed(error);
                },
            });
    }
}
