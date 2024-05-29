/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { EMPTY, Observable, shareReplay } from 'rxjs';
import { AppsState, CodeEditorComponent, EditSchemaScriptsForm, KeysPipe, SchemaDto, SchemasService, SchemasState, ScriptCompletions, ScriptNamePipe, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-schema-scripts-form',
    styleUrls: ['./schema-scripts-form.component.scss'],
    templateUrl: './schema-scripts-form.component.html',
    imports: [
        AsyncPipe,
        CodeEditorComponent,
        FormsModule,
        KeysPipe,
        ReactiveFormsModule,
        ScriptNamePipe,
        TranslatePipe,
    ],
})
export class SchemaScriptsFormComponent {
    @Input()
    public schema!: SchemaDto;

    public schemaScript = 'query';
    public schemaCompletions: Observable<ScriptCompletions> = EMPTY;

    public editForm = new EditSchemaScriptsForm();

    public isEditable = false;

    constructor(
        private readonly appsState: AppsState,
        private readonly schemasState: SchemasState,
        private readonly schemasService: SchemasService,
    ) {
    }

    public ngOnChanges() {
        this.isEditable = this.schema.canUpdateScripts;

        this.editForm.load(this.schema.scripts);
        this.editForm.setEnabled(this.isEditable);

        this.schemaCompletions = this.schemasService.getContentScriptsCompletion(this.appsState.appName, this.schema.name).pipe(shareReplay(1));
    }

    public selectField(field: string) {
        this.schemaScript = field;
    }

    public saveSchema() {
        if (!this.isEditable) {
            return;
        }

        const value = this.editForm.submit();

        if (value) {
            this.schemasState.configureScripts(this.schema, value)
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
}
