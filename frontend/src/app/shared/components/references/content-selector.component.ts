/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, computed, effect, forwardRef, inject, input, linkedSignal, numberAttribute, output, signal, untracked } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { LanguageSelectorComponent, ListViewComponent, ModalDialogComponent, PagerComponent, SyncWidthDirective, TooltipDirective, TranslatePipe } from '@app/framework';
import { AppLanguageDto, AppsState, ComponentContentsState, ContentDto, META_FIELDS, Query, SchemaDto, SchemasService, SchemasState } from '@app/shared/internal';
import { ContentListCellDirective, ContentListWidthDirective } from '../contents/content-list-cell.directive';
import { ContentListHeaderComponent } from '../contents/content-list-header.component';
import { SearchFormComponent } from '../search/search-form.component';
import { ContentSelectorItemComponent } from './content-selector-item.component';

@Component({
    selector: 'sqx-content-selector',
    styleUrls: ['./content-selector.component.scss'],
    templateUrl: './content-selector.component.html',
    providers: [
        ComponentContentsState,
    ],
    imports: [
        ContentListCellDirective,
        ContentListHeaderComponent,
        ContentListWidthDirective,
        ContentSelectorItemComponent,
        LanguageSelectorComponent,
        ListViewComponent,
        ModalDialogComponent,
        PagerComponent,
        SyncWidthDirective,
        TooltipDirective,
        TranslatePipe,
        forwardRef(() => SearchFormComponent),
    ],
})
export class ContentSelectorComponent {
    private readonly appsState = inject(AppsState);
    private readonly schemasService = inject(SchemasService);
    private readonly schemasState = inject(SchemasState);
    private isInitialQueryUsed = false;

    protected readonly contentsState = inject(ComponentContentsState);

    /** Emits the selected contents, or an empty array when the dialog is closed. */
    public readonly contentSelect = output<ReadonlyArray<ContentDto>>();

    /** Emits when the user clears the selection. */
    public readonly contentClear = output();

    /** The maximum number of contents that can be selected. */
    public readonly maxItems = input(Number.MAX_VALUE, { transform: numberAttribute });

    /** The name of the schema. */
    public readonly schemaName = input<string | null>();

    /** The IDs or names of the schemas to select contents from. All schemas are used when not defined. */
    public readonly schemaIdentifiers = input<ReadonlyArray<string>>();

    /** Shows a button to clear the selection. */
    public readonly canClear = input(false, { transform: booleanAttribute });

    /** The initial full text query. */
    public readonly query = input<string>();

    /** The initial language to show the contents in. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Allows to select contents that have already been selected before. */
    public readonly allowDuplicates = input(false, { transform: booleanAttribute });

    /** The IDs of the contents that have already been selected before. */
    public readonly alreadySelectedIds = input<ReadonlyArray<string> | null>();

    /** The contents that have already been selected before. Takes precedence over the IDs. */
    public readonly alreadySelected = input<ReadonlyArray<ContentDto> | null>();

    protected readonly selectedItems = signal<Record<string, ContentDto>>({});

    protected readonly languageSelected = linkedSignal(() => this.language());

    protected readonly schema = signal<SchemaDto | undefined>(undefined);

    protected readonly alreadySelectedIdsAll = computed(() => {
        return this.alreadySelected()?.map(x => x.id) ?? this.alreadySelectedIds();
    });

    protected readonly schemas = computed(() => {
        const schemas = this.schemasState.snapshot.schemas.filter(x => x.type === 'Default' && x.canReadContents);

        const schemaIdentifiers = this.schemaIdentifiers();
        if (!schemaIdentifiers || schemaIdentifiers.length === 0) {
            return schemas;
        }

        return schemas.filter(x => schemaIdentifiers.includes(x.id) || schemaIdentifiers.includes(x.name));
    });

    protected readonly selectionCount = computed(() => {
        return Object.keys(this.selectedItems()).length;
    });

    protected readonly selectedAll = computed(() => {
        return this.selectionCount() === this.contentsState.snapshot.contents.length;
    });

    protected readonly queryModel = rxResource({
        params: () => this.schema()?.name,
        stream: ({ params }) => this.schemasService.getFilters(this.appsState.appName, params),
    });

    protected readonly metaFields = META_FIELDS;

    constructor() {
        effect(() => {
            const schemas = this.schemas();

            untracked(() => this.selectSchema(schemas[0]));
        });
    }

    protected selectSchema(schema: SchemaDto | undefined) {
        this.schema.set(schema);

        if (!schema) {
            return;
        }

        const fullText = !this.isInitialQueryUsed ? this.query() : undefined;

        this.contentsState.schema = schema;
        this.contentsState.search({ fullText: fullText || undefined });

        this.isInitialQueryUsed = true;
    }

    protected reload() {
        this.contentsState.load(true);
    }

    protected reloadTotal() {
        this.contentsState.load(true, false);
    }

    protected search(query: Query) {
        this.contentsState.search(query);
    }

    protected isItemSelected(content: ContentDto) {
        return !!this.selectedItems()[content.id];
    }

    protected isItemAlreadySelected(content: ContentDto) {
        if (this.allowDuplicates()) {
            return false;
        }

        return this.alreadySelectedIdsAll()?.includes(content.id) === true;
    }

    protected emitClose() {
        this.contentSelect.emit([]);
    }

    protected emitClear() {
        this.contentClear.emit();
    }

    protected emitSelect() {
        this.contentSelect.emit(Object.values(this.selectedItems()));
    }

    protected selectAll(isSelected: boolean) {
        const selectedItems: Record<string, ContentDto> = {};

        if (isSelected) {
            for (const content of this.contentsState.snapshot.contents) {
                if (!this.isItemAlreadySelected(content)) {
                    selectedItems[content.id] = content;
                }
            }
        }

        this.selectedItems.set(selectedItems);
    }

    protected selectContent(content: ContentDto) {
        this.selectedItems.update(s => {
            const selectedItems = { ...s };

            if (selectedItems[content.id]) {
                delete selectedItems[content.id];
            } else {
                selectedItems[content.id] = content;
            }

            return selectedItems;
        });
    }
}
