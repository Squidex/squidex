/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

import {
    ContentDto,
    LanguageDto,
    ManualContentsState,
    Query,
    QueryModel,
    queryModelFromSchema,
    ResourceOwner,
    SchemaDetailsDto,
    SchemaDto,
    SchemasState,
    Types
} from '@app/shared';

@Component({
    selector: 'sqx-contents-selector',
    styleUrls: ['./contents-selector.component.scss'],
    templateUrl: './contents-selector.component.html',
    providers: [
        ManualContentsState,
        SchemasState
    ]
})
export class ContentsSelectorComponent extends ResourceOwner implements OnInit {
    @Output()
    public select = new EventEmitter<ContentDto[]>();

    @Input()
    public language: LanguageDto;

    @Input()
    public languages: LanguageDto[];

    @Input()
    public allowDuplicates: boolean;

    @Input()
    public alreadySelected: ContentDto[];

    public schema: SchemaDetailsDto;

    public queryModel: QueryModel;

    public selectedItems:  { [id: string]: ContentDto; } = {};
    public selectionCount = 0;
    public selectedAll = false;

    public minWidth: string;

    constructor(
        public readonly contentsState: ManualContentsState,
        public readonly schemasState: SchemasState
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.contentsState.statuses
                .subscribe(() => {
                    this.updateModel();
                }));

        this.own(
            this.schemasState.selectedSchema
                .subscribe(schema => {
                    this.schema = schema;

                    this.minWidth = `${200 + (200 * schema.referenceFields.length)}px`;

                    this.contentsState.schema = schema;
                    this.contentsState.load();

                    this.updateModel();
                }));

        this.schemasState.load()
            .subscribe(() => {
                this.selectSchema(this.schemasState.snapshot.schemas.at(0));
            });
    }

    public selectSchema(selected: string | SchemaDto) {
        if (Types.is(selected, SchemaDto)) {
            this.schemasState.select(selected.id).subscribe();
        } else {
            this.schemasState.select(selected).subscribe();
        }
    }

    public reload() {
        this.contentsState.load(true);
    }

    public search(query: Query) {
        this.contentsState.search(query);
    }

    public goNext() {
        this.contentsState.goNext();
    }

    public goPrev() {
        this.contentsState.goPrev();
    }

    public isItemSelected(content: ContentDto) {
        return this.selectedItems[content.id];
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
            for (let content of this.contentsState.snapshot.contents.values) {
                this.selectedItems[content.id] = content;
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

    public trackByContent(content: ContentDto): string {
        return content.id;
    }
}