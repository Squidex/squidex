/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { EMPTY, Observable, shareReplay } from 'rxjs';
import { TranslatePipe } from '@app/framework';
import { CodeEditorComponent, ConfigureFieldRulesForm, ConfirmClickDirective, ControlErrorsComponent, FIELD_RULE_ACTIONS, SchemaDto, SchemasService, SchemasState, ScriptCompletions } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-schema-field-rules-form',
    styleUrls: ['./schema-field-rules-form.component.scss'],
    templateUrl: './schema-field-rules-form.component.html',
    imports: [
        AsyncPipe,
        CodeEditorComponent,
        ConfirmClickDirective,
        ControlErrorsComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class SchemaFieldRulesFormComponent implements  OnInit {
    @Input()
    public schema!: SchemaDto;

    public editForm = new ConfigureFieldRulesForm();

    public fieldNames!: ReadonlyArray<string>;
    public fieldActions = FIELD_RULE_ACTIONS;
    public fieldCompletions: Observable<ScriptCompletions> = EMPTY;

    public isEditable = false;

    constructor(
        private readonly schemasState: SchemasState,
        private readonly schemasService: SchemasService,
    ) {
    }

    public ngOnInit() {
        this.fieldCompletions = this.schemasService.getFieldRulesCompletion(this.schemasState.appName, this.schema.name).pipe(shareReplay(1));
    }

    public ngOnChanges() {
        const fieldNames: string[] = [];

        for (const field of this.schema.fields) {
            if (field.properties.isContentField) {
                fieldNames.push(field.name);

                for (const nestedField of field.nested) {
                    if (nestedField.properties.isContentField) {
                        fieldNames.push(`${field.name}.${nestedField.name}`);
                    }
                }
            }
        }

        this.fieldNames = fieldNames.sort();

        this.isEditable = this.schema.canUpdateUrls;

        this.editForm.load(this.schema);
        this.editForm.setEnabled(this.isEditable);
    }

    public add() {
        this.editForm.add(this.fieldNames);
    }

    public saveSchema() {
        if (!this.isEditable) {
            return;
        }

        const value = this.editForm.submit();

        if (value) {
            this.schemasState.configureFieldRules(this.schema, value)
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
