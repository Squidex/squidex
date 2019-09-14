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
    SchemaDetailsDto
} from '@app/shared';

@Component({
    selector: 'sqx-contents-selector',
    styleUrls: ['./contents-selector.component.scss'],
    templateUrl: './contents-selector.component.html',
    providers: [
        ManualContentsState
    ]
})
export class ContentsSelectorComponent extends ResourceOwner implements OnInit {
    @Input()
    public language: LanguageDto;

    @Input()
    public languages: LanguageDto[];

    @Input()
    public allowDuplicates: boolean;

    @Input()
    public alreadySelected: ContentDto[];

    @Input()
    public schema: SchemaDetailsDto;

    @Output()
    public select = new EventEmitter<ContentDto[]>();

    public queryModel: QueryModel;

    public selectedItems:  { [id: string]: ContentDto; } = {};
    public selectionCount = 0;
    public selectedAll = false;

    public minWidth: string;

    constructor(
        public readonly contentsState: ManualContentsState
    ) {
        super();
    }

    public ngOnInit() {
        this.minWidth = `${200 + (200 * this.schema.referenceFields.length)}px`;

        this.own(
            this.contentsState.statuses
                .subscribe(() => {
                    this.updateModel();
                }));

        this.contentsState.schema = this.schema;
        this.contentsState.load();
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
        this.queryModel = queryModelFromSchema(this.schema, this.languages, this.contentsState.snapshot.statuses);
    }

    public trackByContent(content: ContentDto): string {
        return content.id;
    }
}