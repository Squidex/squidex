/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppLanguageDto, ConfirmClickDirective, ControlErrorsComponent, CreateIndexForm, FormHintComponent, IndexesState, MarkdownDirective, ModalDialogComponent, SchemaDto, TranslatePipe } from '@app/shared';


@Component({
    standalone: true,
    selector: 'sqx-index-form',
    styleUrls: ['./index-form.component.scss'],
    templateUrl: './index-form.component.html',
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
    @Output()
    public create = new EventEmitter();

    @Output()
    public dialogClose = new EventEmitter();

    @Input({ required: true })
    public schema!: SchemaDto;

    @Input({ required: true })
    public languages!: AppLanguageDto[];

    public createForm = new CreateIndexForm();
    public fieldNames: string[] = [];

    constructor(
        private readonly indexesState: IndexesState,
    ) {
    }

    public ngOnInit() {
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
        for (const field of this.schema.fields) {
            if (field.properties.isContentField) {
                if (field.isLocalizable) {
                    for (const language of this.languages) {
                        dataFields.push(`data.${field.name}.${language.iso2Code}`);
                    }
                } else {
                    dataFields.push(`data.${field.name}.iv`);
                }
            }
        }

        this.fieldNames = [...metaFields, ...dataFields.sort()];
    }

    public emitCreate() {
        this.create.emit();
    }

    public emitClose() {
        this.dialogClose.emit();
    }

    public createSchema() {
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