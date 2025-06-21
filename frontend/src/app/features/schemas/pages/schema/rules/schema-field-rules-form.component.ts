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
import { CodeEditorComponent, ConfigureFieldRulesForm, ConfirmClickDirective, ControlErrorsComponent, FIELD_RULE_ACTIONS, FieldPropertiesDto, SchemaDto, SchemasService, SchemasState, ScriptCompletions } from '@app/shared';

@Component({
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

    public fieldOptions: ReadonlyArray<{ label: string; value: string }> = [];
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
        const fieldNames = new Set<string>();
        const fieldTags = new Set<string>();

        const addField = (name: string, properties: FieldPropertiesDto) => {
            fieldNames.add(name);

            if (properties.tags) {
                for (const tag of properties.tags) {
                    fieldTags.add(tag);
                }
            }
        };

        for (const field of this.schema.fields) {
            if (field.properties.isContentField) {
                addField(field.name, field.properties);

                for (const nestedField of field.nested || []) {
                    if (nestedField.properties.isContentField) {
                        addField(`${field.name}.${nestedField.name}`, nestedField.properties);
                    }
                }
            }
        }

        this.fieldOptions = [
            ...[...fieldNames]
                .sorted()
                .map(value => ({ label: `field: ${value}`, value })),
            ...[...fieldTags]
                .sorted()
                .map(value => ({ label: `tag: ${value}`, value: `tag:${value}` })),
        ];

        this.isEditable = this.schema.canUpdateUrls;

        this.editForm.load(this.schema);
        this.editForm.setEnabled(this.isEditable);
    }

    public add() {
        this.editForm.add(this.fieldOptions[0].value);
    }

    public saveSchema() {
        if (!this.isEditable) {
            return;
        }

        const value = this.editForm.submit();
        if (!value) {
            return;
        }

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
