/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    EditScriptsForm,
    SchemaDetailsDto,
    SchemasState
} from '@app/shared';

@Component({
    selector: 'sqx-schema-scripts-form',
    styleUrls: ['./schema-scripts-form.component.scss'],
    templateUrl: './schema-scripts-form.component.html'
})
export class SchemaScriptsFormComponent implements OnInit {
    @Output()
    public complete = new EventEmitter();

    @Input()
    public schema: SchemaDetailsDto;

    public selectedField = 'query';

    public editForm = new EditScriptsForm(this.formBuilder);

    public isEditable = false;

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnInit() {
        this.isEditable = this.schema.canUpdateScripts;

        this.editForm.load(this.schema.scripts);
        this.editForm.setEnabled(this.isEditable);
    }

    public emitComplete() {
        this.complete.emit();
    }

    public selectField(field: string) {
        this.selectedField = field;
    }

    public saveSchema() {
        if (!this.isEditable) {
            return;
        }

        const value = this.editForm.submit();

        if (value) {
            this.schemasState.configureScripts(this.schema, value)
                .subscribe(() => {
                    this.emitComplete();
                }, error => {
                    this.editForm.submitFailed(error);
                });
        }
    }
}