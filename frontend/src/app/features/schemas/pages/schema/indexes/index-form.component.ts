/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppLanguageDto, ConfirmClickDirective, ControlErrorsComponent, CreateIndexForm, FormHintComponent, IndexesState, MarkdownDirective, ModalDialogComponent, SchemaDto, TranslatePipe } from '@app/shared';


@Component({
    selector: 'sqx-index-form',
    styleUrls: ['./index-form.component.scss'],
    templateUrl: './index-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        ControlErrorsComponent,
        FormHintComponent,
        FormsModule,
        MarkdownDirective,
        ModalDialogComponent,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class IndexFormComponent {
    private readonly indexesState = inject(IndexesState);

    /** Emits when the index has been created. */
    public readonly create = output();

    /** Emits when the dialog is closed. */
    public readonly dialogClose = output();

    /** The schema to create the index for. */
    public readonly schema = input.required<SchemaDto>();

    /** The languages of the app, to provide the localized fields. */
    public readonly languages = input.required<AppLanguageDto[]>();

    protected readonly createForm = new CreateIndexForm();

    protected readonly fieldNames = computed(() => {
        const languages = this.languages();

        const metaFields: string[] = [
            'created',
            'createdBy',
            'lastModified',
            'lastModifiedBy',
            'newStatus',
            'status',
            'version',
        ];

        const dataFields: string[] = [];
        for (const field of this.schema().fields) {
            if (field.properties.isContentField) {
                if (field.isLocalizable) {
                    for (const language of languages) {
                        dataFields.push(`data.${field.name}.${language.iso2Code}`);
                    }
                } else {
                    dataFields.push(`data.${field.name}.iv`);
                }
            }
        }

        return [...metaFields, ...dataFields.sort()];
    });

    private emitCreate() {
        this.create.emit();
    }

    protected createSchema() {
        const value = this.createForm.submit();
        if (!value) {
            return;
        }

        this.indexesState.create(value)
            .subscribe({
                next: () => {
                    this.emitCreate();
                },
                error: error => {
                    this.createForm.submitFailed(error);
                },
            });
    }
}
