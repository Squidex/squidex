/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnInit } from '@angular/core';
import { EMPTY, Observable, shareReplay } from 'rxjs';
import { ConfigurePreviewUrlsForm, SchemaCompletions, SchemaDto, SchemasService, SchemasState } from '@app/shared';

@Component({
    selector: 'sqx-schema-preview-urls-form',
    styleUrls: ['./schema-preview-urls-form.component.scss'],
    templateUrl: './schema-preview-urls-form.component.html',
})
export class SchemaPreviewUrlsFormComponent implements OnInit {
    @Input()
    public schema!: SchemaDto;

    public editForm = new ConfigurePreviewUrlsForm();

    public fieldCompletions: Observable<SchemaCompletions> = EMPTY;

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
        this.isEditable = this.schema.canUpdateUrls;

        this.editForm.load(this.schema);
        this.editForm.setEnabled(this.isEditable);
    }

    public saveSchema() {
        if (!this.isEditable) {
            return;
        }

        const value = this.editForm.submit();

        if (value) {
            this.schemasState.configurePreviewUrls(this.schema, value)
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
