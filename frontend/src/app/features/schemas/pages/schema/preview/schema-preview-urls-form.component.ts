/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, inject, input } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { distinctUntilChanged, map, switchMap } from 'rxjs';
import { CodeEditorComponent, ConfigurePreviewUrlsForm, ConfirmClickDirective, ControlErrorsComponent, FormAlertComponent, SchemaDto, SchemasService, SchemasState, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-schema-preview-urls-form',
    styleUrls: ['./schema-preview-urls-form.component.scss'],
    templateUrl: './schema-preview-urls-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
    private readonly schemasState = inject(SchemasState);

    /** The schema to configure the preview URLs for. */
    public readonly schema = input.required<SchemaDto>();

    protected readonly editForm = new ConfigurePreviewUrlsForm();

    protected readonly fieldCompletions;

    protected readonly isEditable = computed(() => {
        return this.schema().canUpdateUrls;
    });

    constructor() {
        const schemasService = inject(SchemasService);
        const schemasState = this.schemasState;

        this.fieldCompletions = toSignal(
            toObservable(this.schema).pipe(
                map(schema => schema.name),
                distinctUntilChanged(),
                switchMap(name => schemasService.getFieldRulesCompletion(schemasState.appName, name))));

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

        const value = this.editForm.submit();
        if (!value) {
            return;
        }

        this.schemasState.configurePreviewUrls(this.schema(), value)
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
