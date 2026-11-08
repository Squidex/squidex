/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, effect, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { CodeEditorComponent, ConfigurePreviewUrlsForm, ConfirmClickDirective, ControlErrorsComponent, FormAlertComponent, SchemaDto, SchemasService, SchemasState, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-schema-preview-urls-form',
    styleUrls: ['./schema-preview-urls-form.component.scss'],
    templateUrl: './schema-preview-urls-form.component.html',
    imports: [
        CodeEditorComponent,
        ConfirmClickDirective,
        ControlErrorsComponent,
        FormAlertComponent,
        FormField,
        FormsModule,
        TranslatePipe,
    ],
})
export class SchemaPreviewUrlsFormComponent {
    private readonly schemasService = inject(SchemasService);
    private readonly schemasState = inject(SchemasState);

    /** The schema to configure the preview URLs for. */
    public readonly schema = input.required<SchemaDto>();

    protected readonly isEditable = computed(() => {
        return this.schema().canUpdateUrls;
    });

    protected readonly fieldCompletions = rxResource({
        params: () => this.schema().name,
        stream: ({ params }) => this.schemasService.getFieldRulesCompletion(this.schemasState.appName, params),
    });

    protected readonly editForm = new ConfigurePreviewUrlsForm();

    constructor() {
        effect(() => {
            const schema = this.schema();

            this.editForm.load(schema);
            this.editForm.setEnabled(schema.canUpdateUrls);
        });
    }

    protected saveSchema() {
        if (!this.isEditable()) {
            return;
        }

        this.editForm.submit(value => {
            return this.schemasState.configurePreviewUrls(this.schema(), value);
        });
    }
}
