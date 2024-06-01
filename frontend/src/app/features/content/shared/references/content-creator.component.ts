/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, EventEmitter, forwardRef, Input, OnInit, Output } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppLanguageDto, ComponentContentsState, ContentDto, EditContentForm, FormErrorComponent, LanguageSelectorComponent, ModalDialogComponent, ResolveAssets, ResolveContents, SchemaDto, SchemasState, TooltipDirective, TranslatePipe } from '@app/shared';
import { ContentSectionComponent } from '../forms/content-section.component';

@Component({
    standalone: true,
    selector: 'sqx-content-creator',
    styleUrls: ['./content-creator.component.scss'],
    templateUrl: './content-creator.component.html',
    providers: [
        ResolveAssets,
        ResolveContents,
        ComponentContentsState,
    ],
    imports: [
        AsyncPipe,
        FormErrorComponent,
        FormsModule,
        LanguageSelectorComponent,
        ModalDialogComponent,
        ReactiveFormsModule,
        TooltipDirective,
        TranslatePipe,
        forwardRef(() => ContentSectionComponent),
    ],
})
export class ContentCreatorComponent implements OnInit {
    @Output()
    public contentCreate = new EventEmitter<ReadonlyArray<ContentDto>>();

    @Input()
    public initialData: any;

    @Input()
    public schemaName?: string | null;

    @Input()
    public schemaIds?: ReadonlyArray<string>;

    @Input({ required: true })
    public language!: AppLanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    @Input({ required: true })
    public formContext!: any;

    public schema!: SchemaDto;
    public schemas: ReadonlyArray<SchemaDto> = [];

    public contentForm!: EditContentForm;

    constructor(
        private readonly contentsState: ComponentContentsState,
        private readonly schemasState: SchemasState,
    ) {
    }

    public ngOnInit() {
        this.schemas = this.schemasState.snapshot.schemas.filter(x => x.canContentsCreate);

        if (this.schemaIds && this.schemaIds.length > 0) {
            this.schemas = this.schemas.filter(x => x.type === 'Default' && this.schemaIds!.indexOf(x.id) >= 0);
        }

        const selectedSchema = this.schemas.find(x => x.name === this.schemaName) || this.schemas[0];

        this.selectSchema(selectedSchema);
    }

    public selectSchema(schema: SchemaDto) {
        this.schema = schema;

        if (schema) {
            this.contentsState.schema = schema;
            this.contentForm = new EditContentForm(this.languages, this.schema, this.schemasState.schemaMap, { user: this.formContext.user });

            this.formContext['languages'] = this.languages;
            this.formContext['schema'] = schema;

            if (this.initialData) {
                this.contentForm.load(this.initialData, true);

                this.initialData = null;
            }
        }
    }

    public saveAndPublish() {
        this.saveContent(true);
    }

    public save() {
        this.saveContent(false);
    }

    private saveContent(publish: boolean) {
        const value = this.contentForm.submit();

        if (value) {
            if (!this.canCreate(publish)) {
                return;
            }

            this.contentsState.create(value, publish)
                .subscribe({
                    next: content => {
                        this.contentForm.submitCompleted({ noReset: true });

                        this.emitSelect(content);
                    },
                    error: error => {
                        this.contentForm.submitFailed(error);
                    },
                });
        } else {
            this.contentForm.submitFailed('i18n:contents.contentNotValid');
        }
    }

    private canCreate(publish: boolean) {
        if (publish) {
            return this.schema.canContentsCreateAndPublish;
        } else {
            return this.schema.canContentsCreate;
        }
    }

    public emitClose() {
        this.contentCreate.emit([]);
    }

    public emitSelect(content: ContentDto) {
        this.contentCreate.emit([content]);
    }
}
