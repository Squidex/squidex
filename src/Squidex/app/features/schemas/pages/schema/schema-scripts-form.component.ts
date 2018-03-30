/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import { SchemaDetailsDto } from '@app/shared';
import { SchemasState, EditScriptsForm } from './../../state/schemas.state';

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

    public editForm: EditScriptsForm;

    constructor(formBuilder: FormBuilder,
        private readonly schemasState: SchemasState
    ) {
        this.editForm = new EditScriptsForm(formBuilder);
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