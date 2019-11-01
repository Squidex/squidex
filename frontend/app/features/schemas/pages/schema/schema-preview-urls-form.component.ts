/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnChanges } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AddPreviewUrlForm,
    ConfigurePreviewUrlsForm,
    DialogService,
    SchemaDetailsDto,
    SchemasState
} from '@app/shared';

@Component({
    selector: 'sqx-schema-preview-urls-form',
    styleUrls: ['./schema-preview-urls-form.component.scss'],
    templateUrl: './schema-preview-urls-form.component.html'
})
export class SchemaPreviewUrlsFormComponent implements OnChanges {
    @Input()
    public schema: SchemaDetailsDto;

    public addForm = new AddPreviewUrlForm(this.formBuilder);

    public editForm = new ConfigurePreviewUrlsForm(this.formBuilder);

    public isEditable = false;

    constructor(
        private readonly dialogs: DialogService,
        private readonly formBuilder: FormBuilder,
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnChanges() {
        this.isEditable = this.schema.canUpdateUrls;

        this.editForm.load(this.schema.previewUrls);
        this.editForm.setEnabled(this.isEditable);
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
                    this.dialogs.notifyInfo('Preview URLs successfully.');

                    this.editForm.submitCompleted({ noReset: true });
                }, error => {
                    this.editForm.submitFailed(error);
                });
        }
    }
}