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
    hasAnyLink,
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
    public complete = new EventEmitter();

    @Input()
    public schema: SchemaDetailsDto;

    public addForm = new AddPreviewUrlForm(this.formBuilder);

    public editForm = new ConfigurePreviewUrlsForm(this.formBuilder);

    public isEditable = false;

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnInit() {
        this.isEditable = hasAnyLink(this.schema, 'update');

        this.editForm.load(this.schema.previewUrls);

        if (!this.isEditable) {
            return;
        }
    }

    public emitComplete() {
        this.complete.emit();
    }

    public cancelAdd() {
        this.addForm.submitCompleted();
    }

    public add() {
        if (!this.isEditable) {
            return;
        }

        const value = this.addForm.submit();

        if (value) {
            this.editForm.add(value);

            this.cancelAdd();
        }
    }

    public saveSchema() {
        if (!this.isEditable) {
            return;
        }

        const value = this.editForm.submit();

        if (value) {
            this.schemasState.configurePreviewUrls(this.schema, value)
                .subscribe(() => {
                    this.emitComplete();
                }, error => {
                    this.editForm.submitFailed(error);
                });
        }
    }
}