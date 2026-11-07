/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, forwardRef, input, linkedSignal, output, signal, untracked } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppLanguageDto, ComponentContentsState, ContentDto, EditContentForm, FormErrorComponent, LanguageSelectorComponent, ModalDialogComponent, ResolveAssets, ResolveContents, SchemaDto, SchemasState, TooltipDirective, TranslatePipe } from '@app/shared';
import { ContentSectionComponent } from '../forms/content-section.component';

@Component({
    selector: 'sqx-content-creator',
    styleUrls: ['./content-creator.component.scss'],
    templateUrl: './content-creator.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        ResolveAssets,
        ResolveContents,
        ComponentContentsState,
    ],
    imports: [
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
export class ContentCreatorComponent {
    private isInitialDataLoaded = false;

    /** Emits the created content, or an empty array when the dialog is closed. */
    public readonly contentCreate = output<ReadonlyArray<ContentDto>>();

    /** The data to initialize the new content with, for example when cloning a content. */
    public readonly initialData = input<any>();

    /** The name of the schema to select initially. */
    public readonly schemaName = input<string | null>();

    /** The IDs of the schemas the user can create contents for. */
    public readonly schemaIds = input<ReadonlyArray<string>>();

    /** The initial language of the editor. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Additional context information that is passed to custom editors. */
    public readonly formContext = input.required<any>();

    protected readonly languageSelected = linkedSignal(() => {
        return this.language();
    });

    protected readonly schemas = computed(() => {
        const schemaIds = this.schemaIds();

        const schemas = this.schemasState.snapshot.schemas.filter(x => x.canContentsCreate);
        if (!schemaIds || schemaIds.length === 0) {
            return schemas;
        }

        return schemas.filter(x => x.type === 'Default' && schemaIds.indexOf(x.id) >= 0);
    });

    protected readonly schema = linkedSignal<SchemaDto | undefined>(() => {
        const schemas = this.schemas();

        return schemas.find(x => x.name === this.schemaName()) || schemas[0];
    });

    protected readonly contentForm = signal<EditContentForm | undefined>(undefined);

    constructor(
        private readonly contentsState: ComponentContentsState,
        private readonly schemasState: SchemasState,
    ) {
        effect(() => {
            const schema = this.schema();
            if (!schema) {
                return;
            }

            untracked(() => this.createForm(schema));
        });
    }

    protected selectSchema(schema: SchemaDto) {
        this.schema.set(schema);
    }

    private createForm(schema: SchemaDto) {
        const formContext = this.formContext();
        const languages = this.languages();

        this.contentsState.schema = schema;

        const contentForm = new EditContentForm(languages, schema, this.schemasState.schemaMap, { user: formContext.user });

        formContext['languages'] = languages;
        formContext['schema'] = schema;

        const initialData = this.initialData();
        if (initialData && !this.isInitialDataLoaded) {
            contentForm.load(initialData, true);

            this.isInitialDataLoaded = true;
        }

        this.contentForm.set(contentForm);
    }

    protected saveAndPublish() {
        this.saveContent(true);
    }

    protected save() {
        this.saveContent(false);
    }

    private saveContent(publish: boolean) {
        const contentForm = this.contentForm();
        if (!contentForm) {
            return;
        }

        const value = contentForm.submit();
        if (!value) {
            contentForm.submitFailed('i18n:contents.contentNotValid');
            return;
        }

        if (!this.canCreate(publish)) {
            return;
        }

        this.contentsState.create(value, publish)
            .subscribe({
                next: content => {
                    contentForm.submitCompleted({ noReset: true });

                    this.emitSelect(content);
                },
                error: error => {
                    contentForm.submitFailed(error);
                },
            });
    }

    private canCreate(publish: boolean) {
        const schema = this.schema();
        if (!schema) {
            return false;
        }

        if (publish) {
            return schema.canContentsCreateAndPublish;
        } else {
            return schema.canContentsCreate;
        }
    }

    protected emitClose() {
        this.contentCreate.emit([]);
    }

    private emitSelect(content: ContentDto) {
        this.contentCreate.emit([content]);
    }
}
