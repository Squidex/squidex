/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { AddFieldRuleForm, ConfigureFieldRulesForm, SchemaDetailsDto, SchemasState } from '@app/shared';

@Component({
    selector: 'sqx-schema-field-rules-form',
    styleUrls: ['./schema-field-rules-form.component.scss'],
    templateUrl: './schema-field-rules-form.component.html'
})
export class SchemaFieldRulesFormComponent implements OnChanges {
    @Input()
    public schema: SchemaDetailsDto;

    public formAdd = new AddFieldRuleForm(this.formBuilder);
    public formEdit = new ConfigureFieldRulesForm(this.formBuilder);

    public fieldNames: ReadonlyArray<string>;

    public isEditable = false;

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnChanges() {
        this.isEditable = this.schema.canUpdateUrls;

        this.formEdit.load(this.schema);
        this.formEdit.setEnabled(this.isEditable);

        const fieldNames: string[] = [];

        this.fieldNames = fieldNames;
    }

    public cancelAdd() {
        this.formAdd.submitCompleted();
    }

    public add() {
        if (!this.isEditable) {
            return;
        }

        const value = this.formAdd.submit();

        if (value) {
            this.formEdit.add(value);

            this.cancelAdd();
        }
    }

    public saveSchema() {
        if (!this.isEditable) {
            return;
        }

        const value = this.formEdit.submit();

        if (value) {
            this.schemasState.configureFieldRules(this.schema, value)
                .subscribe(() => {
                    this.formEdit.submitCompleted({ noReset: true });
                }, error => {
                    this.formEdit.submitFailed(error);
                });
        }
    }
}