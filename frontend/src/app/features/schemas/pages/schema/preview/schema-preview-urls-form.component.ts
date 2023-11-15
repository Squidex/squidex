/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { EMPTY, Observable, shareReplay } from 'rxjs';
import { ConfigurePreviewUrlsForm, SchemaDto, SchemasService, SchemasState, ScriptCompletions } from '@app/shared';
import { TranslatePipe } from '@app/shared';
import { ConfirmClickDirective } from '@app/shared';
import { CodeEditorComponent } from '@app/shared';
import { ControlErrorsComponent } from '@app/shared';
import { FormAlertComponent } from '@app/shared';

@Component({
    selector: 'sqx-schema-preview-urls-form',
    styleUrls: ['./schema-preview-urls-form.component.scss'],
    templateUrl: './schema-preview-urls-form.component.html',
    standalone: true,
    imports: [
        FormsModule,
        FormAlertComponent,
        NgIf,
        NgFor,
        ReactiveFormsModule,
        ControlErrorsComponent,
        CodeEditorComponent,
        ConfirmClickDirective,
        AsyncPipe,
        TranslatePipe,
    ],
})
export class SchemaPreviewUrlsFormComponent implements OnInit {
    @Input()
    public schema!: SchemaDto;

    public editForm = new ConfigurePreviewUrlsForm();

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
