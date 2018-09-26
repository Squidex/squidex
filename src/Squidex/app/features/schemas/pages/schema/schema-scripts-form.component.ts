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
    public completed = new EventEmitter();

    @Input()
    public schema: SchemaDetailsDto;

    public selectedField = 'scriptQuery';

    public editForm = new EditScriptsForm(this.formBuilder);

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnInit() {
        this.editForm.load(this.schema);
    }

    public complete() {
        this.completed.emit();
    }

    public selectField(field: string) {
        this.selectedField = field;
    }

    public saveSchema() {
        const value = this.editForm.submit();

        if (value) {
            this.schemasState.configureScripts(this.schema, value)
                .subscribe(dto => {
                    this.complete();
                }, error => {
                    this.editForm.submitFailed(error);
                });
        }
    }
}