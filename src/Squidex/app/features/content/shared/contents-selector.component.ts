/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

import {
    AppsState,
    ContentDto,
    ContentsService,
    DialogService,
    ImmutableArray,
    LanguageDto,
    ModalView,
    Pager,
    SchemaDetailsDto
} from '@app/shared';

@Component({
    selector: 'sqx-contents-selector',
    styleUrls: ['./contents-selector.component.scss'],
    templateUrl: './contents-selector.component.html'
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

    public searchModal = new ModalView();

    public contentItems: ImmutableArray<ContentDto>;
    public contentsQuery = '';
    public contentsPager = new Pager(0);

    public selectedItems:  { [id: string]: ContentDto; } = {};
    public selectionCount = 0;

    public isAllSelected = false;

    constructor(
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly dialogs: DialogService
    ) {
    }

    public ngOnInit() {
        this.load();
    }

    public reload() {
        this.load(true);
    }

    private load(notifyLod = false) {
        this.contentsService.getContents(this.appsState.appName, this.schema.name, this.contentsPager.pageSize, this.contentsPager.skip, this.contentsQuery, undefined, false)
            .finally(() => {
                this.selectedItems = {};

                this.updateSelectionSummary();
            })
            .subscribe(dtos => {
                this.contentItems = ImmutableArray.of(dtos.items);
                this.contentsPager = this.contentsPager.setCount(dtos.total);

                if (notifyLod) {
                    this.dialogs.notifyInfo('Contents reloaded.');
                }
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public search(query: string) {
        this.contentsQuery = query;
        this.contentsPager = new Pager(0);

        this.load();
    }

    public goNext() {
        this.contentsPager = this.contentsPager.goNext();

        this.load();
    }

    public goPrev() {
        this.contentsPager = this.contentsPager.goPrev();

        this.load();
    }

    public isItemSelected(content: ContentDto) {
        return this.selectedItems[content.id];
    }

    public selectLanguage(language: LanguageDto) {
        this.language = language;
    }

    public complete() {
        this.selected.emit([]);
    }

    public select() {
        this.selected.emit(Object.values(this.selectedItems));
    }

    public selectAll(isSelected: boolean) {
        this.selectedItems = {};

        if (isSelected) {
            for (let content of this.contentItems.values) {
                this.selectedItems[content.id] = content;
            }
        }

        this.updateSelectionSummary();
    }

    public onContentSelected(content: ContentDto) {
        if (this.selectedItems[content.id]) {
            delete this.selectedItems[content.id];
        } else {
            this.selectedItems[content.id] = content;
        }

        this.updateSelectionSummary();
    }

    private updateSelectionSummary() {
        this.selectionCount = Object.keys(this.selectedItems).length;

        this.isAllSelected = this.selectionCount === this.contentItems.length;
    }

    public trackByContent(content: ContentDto): string {
        return content.id;
    }
}

