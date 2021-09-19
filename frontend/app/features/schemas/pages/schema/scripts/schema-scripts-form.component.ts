/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { AppsState, EditSchemaScriptsForm, SchemaCompletions, SchemaDto, SchemasService, SchemasState } from '@app/shared';
import { EMPTY, Observable } from 'rxjs';

@Component({
    selector: 'sqx-schema-scripts-form',
    styleUrls: ['./schema-scripts-form.component.scss'],
    templateUrl: './schema-scripts-form.component.html',
})
export class SchemaScriptsFormComponent implements OnChanges {
    @Input()
    public schema: SchemaDto;

    public schemaScript = 'query';
    public schemaCompletions: Observable<SchemaCompletions> = EMPTY;

    public editForm = new EditSchemaScriptsForm(this.formBuilder);

    public isEditable = false;

    constructor(
        private readonly appsState: AppsState,
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState,
        private readonly schemasService: SchemasService,
    ) {
    }

    public ngOnChanges() {
        this.isEditable = this.schema.canUpdateScripts;

        this.editForm.load(this.schema.scripts);
        this.editForm.setEnabled(this.isEditable);

        this.schemaCompletions = this.schemasService.getCompletions(this.appsState.appName, this.schema.name);
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
