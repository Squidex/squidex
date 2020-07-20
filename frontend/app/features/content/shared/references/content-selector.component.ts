/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ApiUrlConfig, AppsState, ContentDto, LanguageDto, ManualContentsState, Query, QueryModel, queryModelFromSchema, ResourceOwner, SchemaDetailsDto, SchemaDto, SchemasState, Types } from '@app/shared';

@Component({
    selector: 'sqx-content-selector',
    styleUrls: ['./content-selector.component.scss'],
    templateUrl: './content-selector.component.html',
    providers: [
        ManualContentsState
    ]
})
export class ContentSelectorComponent extends ResourceOwner implements OnInit {
    @Output()
    public select = new EventEmitter<ReadonlyArray<ContentDto>>();

    @Input()
    public schemaIds: ReadonlyArray<string>;

    @Input()
    public language: LanguageDto;

    @Input()
    public languages: ReadonlyArray<LanguageDto>;

    @Input()
    public allowDuplicates: boolean;

    @Input()
    public alreadySelected: ReadonlyArray<ContentDto>;

    public schema: SchemaDetailsDto;
    public schemas: ReadonlyArray<SchemaDto> = [];

    public queryModel: QueryModel;

    public selectedItems:  { [id: string]: ContentDto; } = {};
    public selectionCount = 0;
    public selectedAll = false;

    constructor(
        public readonly appsState: AppsState,
        public readonly apiUrl: ApiUrlConfig,
        public readonly contentsState: ManualContentsState,
        public readonly schemasState: SchemasState,
        private readonly changeDetector: ChangeDetectorRef
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.contentsState.statuses
                .subscribe(() => {
                    this.updateModel();
                }));

        this.schemas = this.schemasState.snapshot.schemas;

        if (this.schemaIds && this.schemaIds.length > 0) {
            this.schemas = this.schemas.filter(x => this.schemaIds.indexOf(x.id) >= 0);
        }

        this.selectSchema(this.schemas[0]);
    }

    public selectSchema(selected: string | SchemaDto) {
        if (Types.is(selected, SchemaDto)) {
            selected = selected.id;
        }

        this.schemasState.loadSchema(selected, true)
            .subscribe(schema => {
                if (schema) {
                    this.schema = schema;

                    this.contentsState.schema = schema;
                    this.contentsState.load();

                    this.updateModel();

                    this.changeDetector.markForCheck();
                }
            });
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

    public selectLanguage(language: LanguageDto) {
        this.language = language;
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