/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AddPreviewUrlForm,
    ConfigurePreviewUrlsForm,
    SchemaDetailsDto,
    SchemasState
} from '@app/shared';

@Component({
    selector: 'sqx-schema-preview-urls-form',
    styleUrls: ['./schema-preview-urls-form.component.scss'],
    templateUrl: './schema-preview-urls-form.component.html'
})
export class SchemaPreviewUrlsFormComponent implements OnInit {
    @Output()
    public completed = new EventEmitter();

    @Input()
    public schema: SchemaDetailsDto;

    public addForm = new AddPreviewUrlForm(this.formBuilder);

    public editForm = new ConfigurePreviewUrlsForm(this.formBuilder);

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnInit() {
        this.editForm.load(this.schema.previewUrls);
    }

    public complete() {
        this.completed.emit();
    }

    public cancelAdd() {
        this.addForm.submitCompleted({});
    }

    public add() {
        const value = this.addForm.submit();

        if (value) {
            this.editForm.add(value);

            this.addForm.submitCompleted({});
        }
    }

    public saveSchema() {
        const value = this.editForm.submit();

        if (value) {
            this.schemasState.configurePreviewUrls(this.schema, value)
                .subscribe(() => {
                    this.complete();
                }, error => {
                    this.editForm.submitFailed(error);
                });
        }
    }
}