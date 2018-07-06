/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { onErrorResumeNext } from 'rxjs/operators';

import {
    ContentDto,
    LanguageDto,
    ManualContentsState,
    ModalModel,
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
export class ContentsSelectorComponent implements OnInit {
    @Input()
    public language: LanguageDto;

    @Input()
    public languages: LanguageDto[];

    @Input()
    public schema: SchemaDetailsDto;

    @Output()
    public selected = new EventEmitter<ContentDto[]>();

    public searchModal = new ModalModel();

    public selectedItems:  { [id: string]: ContentDto; } = {};
    public selectionCount = 0;

    public isAllSelected = false;

    constructor(
        public readonly contentsState: ManualContentsState
    ) {
    }

    public ngOnInit() {
        this.contentsState.schema = this.schema;

        this.contentsState.load().pipe(onErrorResumeNext()).subscribe();
    }

    public reload() {
        this.contentsState.load(true).pipe(onErrorResumeNext()).subscribe();
    }

    public search(query: string) {
        this.contentsState.search(query).pipe(onErrorResumeNext()).subscribe();
    }

    public goNext() {
        this.contentsState.goNext().pipe(onErrorResumeNext()).subscribe();
    }

    public goPrev() {
        this.contentsState.goPrev().pipe(onErrorResumeNext()).subscribe();
    }

    public isItemSelected(content: ContentDto) {
        return this.selectedItems[content.id];
    }

    public complete() {
        this.selected.emit([]);
    }

    public select() {
        this.selected.emit(Object.values(this.selectedItems));
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

        this.isAllSelected = this.selectionCount === this.contentsState.snapshot.contents.length;
    }

    public trackByContent(content: ContentDto): string {
        return content.id;
    }
}

