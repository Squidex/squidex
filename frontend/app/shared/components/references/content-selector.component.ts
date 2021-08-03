/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ApiUrlConfig, AppsState, ComponentContentsState, ContentDto, LanguageDto, Query, QueryModel, queryModelFromSchema, ResourceOwner, SchemaDto, SchemasState } from '@app/shared/internal';

@Component({
    selector: 'sqx-content-selector[language][languages]',
    styleUrls: ['./content-selector.component.scss'],
    templateUrl: './content-selector.component.html',
    providers: [
        ComponentContentsState,
    ],
})
export class ContentSelectorComponent extends ResourceOwner implements OnInit {
    @Output()
    public select = new EventEmitter<ReadonlyArray<ContentDto>>();

    @Input()
    public maxItems = Number.MAX_VALUE;

    @Input()
    public schemaName?: string | null;

    @Input()
    public schemaIds?: ReadonlyArray<string>;

    @Input()
    public language: LanguageDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    @Input()
    public allowDuplicates?: boolean | null;

    @Input()
    public alreadySelected: ReadonlyArray<ContentDto> | undefined | null;

    public schema: SchemaDto;
    public schemas: ReadonlyArray<SchemaDto> = [];

    public queryModel: QueryModel;

    public selectedItems: { [id: string]: ContentDto } = {};
    public selectionCount = 0;
    public selectedAll = false;

    constructor(
        public readonly appsState: AppsState,
        public readonly apiUrl: ApiUrlConfig,
        public readonly contentsState: ComponentContentsState,
        public readonly schemasState: SchemasState,
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.contentsState.statuses
                .subscribe(() => {
                    this.updateModel();
                }));

        this.schemas = this.schemasState.snapshot.schemas.filter(x => x.canReadContents);

        if (this.schemaIds && this.schemaIds.length > 0) {
            this.schemas = this.schemas.filter(x => x.canReadContents && this.schemaIds!.indexOf(x.id) >= 0);
        }

        this.selectSchema(this.schemas[0]);
    }

    public selectSchema(schema: SchemaDto) {
        this.schema = schema;

        if (schema) {
            this.contentsState.schema = schema;
            this.contentsState.load();

            this.updateModel();
        }
    }

    public reload() {
        this.contentsState.load(true);
    }

    public search(query: Query) {
        this.contentsState.search(query);
    }

    public isItemSelected(content: ContentDto) {
        return !!this.selectedItems[content.id];
    }

    public isItemAlreadySelected(content: ContentDto) {
        return !this.allowDuplicates && this.alreadySelected && !!this.alreadySelected.find(x => x.id === content.id);
    }

    public emitComplete() {
        this.select.emit([]);
    }

    public emitSelect() {
        this.select.emit(Object.values(this.selectedItems));
    }

    public selectAll(isSelected: boolean) {
        this.selectedItems = {};

        if (isSelected) {
            for (const content of this.contentsState.snapshot.contents) {
                if (!this.isItemAlreadySelected(content)) {
                    this.selectedItems[content.id] = content;
                }
            }
        }

        this.updateSelectionSummary();
    }

    public selectContent(content: ContentDto) {
        if (this.selectedItems[content.id]) {
            delete this.selectedItems[content.id];
        } else {
            this.selectedItems[content.id] = content;
        }

        this.updateSelectionSummary();
    }

    private updateSelectionSummary() {
        this.selectionCount = Object.keys(this.selectedItems).length;
        this.selectedAll = this.selectionCount === this.contentsState.snapshot.contents.length;
    }

    private updateModel() {
        if (this.schema) {
            this.queryModel = queryModelFromSchema(this.schema, this.languages, this.contentsState.snapshot.statuses);
        }
    }

    public trackByContent(_index: number, content: ContentDto): string {
        return content.id;
    }
}
