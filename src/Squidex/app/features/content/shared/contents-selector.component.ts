/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

import {
    ContentDto,
    FilterState,
    LanguageDto,
    ManualContentsState,
    RootFieldDto,
    SchemaDetailsDto,
    Sorting
} from '@app/shared';

@Component({
    selector: 'sqx-contents-selector',
    styleUrls: ['./contents-selector.component.scss'],
    templateUrl: './contents-selector.component.html',
    providers: [
        ManualContentsState
    ]
})
export class ContentsSelectorComponent implements OnInit {
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

    public filter = new FilterState();

    public selectedItems:  { [id: string]: ContentDto; } = {};
    public selectionCount = 0;

    public isAllSelected = false;

    constructor(
        public readonly contentsState: ManualContentsState
    ) {
    }

    public ngOnInit() {
        this.contentsState.schema = this.schema;

        this.contentsState.load();
    }

    public reload() {
        this.contentsState.load(true);
    }

    public search() {
        this.contentsState.search(this.filter.apiFilter);
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

    public sort(field: string | RootFieldDto, sorting: Sorting) {
        this.filter.setOrderField(field, sorting);

        this.search();
    }

    private updateSelectionSummary() {
        this.selectionCount = Object.keys(this.selectedItems).length;

        this.isAllSelected = this.selectionCount === this.contentsState.snapshot.contents.length;
    }

    public trackByContent(index: number, content: ContentDto): string {
        return content.id;
    }
}

