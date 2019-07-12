/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit, ViewChild } from '@angular/core';
import { onErrorResumeNext, switchMap, tap } from 'rxjs/operators';

import {
    AppLanguageDto,
    AppsState,
    ContentDto,
    ContentsState,
    FilterState,
    ImmutableArray,
    LanguagesState,
    ModalModel,
    Queries,
    ResourceOwner,
    RootFieldDto,
    SchemaDetailsDto,
    SchemasState,
    Sorting,
    UIState
} from '@app/shared';

import { DueTimeSelectorComponent } from './../../shared/due-time-selector.component';

@Component({
    selector: 'sqx-contents-page',
    styleUrls: ['./contents-page.component.scss'],
    templateUrl: './contents-page.component.html'
})
export class ContentsPageComponent extends ResourceOwner implements OnInit {
    public schema: SchemaDetailsDto;
    public schemaQueries: Queries;

    public searchModal = new ModalModel();

    public selectedItems:  { [id: string]: boolean; } = {};
    public selectionCount = 0;
    public selectionCanDelete = false;

    public nextStatuses: string[] = [];

    public language: AppLanguageDto;
    public languages: ImmutableArray<AppLanguageDto>;

    public filter = new FilterState();

    public isAllSelected = false;

    @ViewChild('dueTimeSelector', { static: false })
    public dueTimeSelector: DueTimeSelectorComponent;

    constructor(
        public readonly appsState: AppsState,
        public readonly contentsState: ContentsState,
        private readonly languagesState: LanguagesState,
        private readonly schemasState: SchemasState,
        private readonly uiState: UIState
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.schemasState.selectedSchema
                .subscribe(schema => {
                    this.filter = new FilterState();
                    this.filter.setLanguage(this.language);

                    this.resetSelection();

                    this.schema = schema!;
                    this.schemaQueries = new Queries(this.uiState, `schemas.${this.schema.name}`);

                    this.contentsState.load();
                }));

        this.own(
            this.contentsState.contentsQuery
                .subscribe(query => {
                    this.filter.setQuery(query);
                }));

        this.own(
            this.contentsState.contents
                .subscribe(() => {
                    this.updateSelectionSummary();
                }));

        this.own(
            this.languagesState.languages
                .subscribe(languages => {
                    this.languages = languages.map(x => x.language);
                    this.language = this.languages.at(0);

                    this.filter.setLanguage(this.language);
                }));
    }

    public reload() {
        this.contentsState.load(true);
    }

    public deleteSelected() {
        this.contentsState.deleteMany(this.selectItems());
    }

    public delete(content: ContentDto) {
        this.contentsState.deleteMany([content]);
    }

    public changeStatus(content: ContentDto, status: string) {
        this.changeContentItems([content], status);
    }

    public changeSelectedStatus(status: string) {
        this.changeContentItems(this.selectItems(c => c.status !== status), status);
    }

    public clone(content: ContentDto) {
        this.contentsState.create(content.dataDraft, false);
    }

    private changeContentItems(contents: ContentDto[], action: string) {
        if (contents.length === 0) {
            return;
        }

        this.dueTimeSelector.selectDueTime(action).pipe(
                tap(() => {
                    this.resetSelection();
                }),
                switchMap(d => this.contentsState.changeManyStatus(contents, action, d)),
                onErrorResumeNext())
            .subscribe();
    }

    public goPrev() {
        this.contentsState.goPrev();
    }

    public goNext() {
        this.contentsState.goNext();
    }

    public search() {
        this.contentsState.search(this.filter.apiFilter);
    }

    public selectLanguage(language: AppLanguageDto) {
        this.language = language;
    }

    public isItemSelected(content: ContentDto): boolean {
        return !!this.selectedItems[content.id];
    }

    private selectItems(predicate?: (content: ContentDto) => boolean) {
        return this.contentsState.snapshot.contents.values.filter(c => this.selectedItems[c.id] && (!predicate || predicate(c)));
    }

    public selectItem(content: ContentDto, isSelected: boolean) {
        this.selectedItems[content.id] = isSelected;

        this.updateSelectionSummary();
    }

    private resetSelection() {
        this.selectedItems = {};

        this.updateSelectionSummary();
    }

    public selectAll(isSelected: boolean) {
        this.selectedItems = {};

        if (isSelected) {
            for (let content of this.contentsState.snapshot.contents.values) {
                this.selectedItems[content.id] = true;
            }
        }

        this.updateSelectionSummary();
    }

    public sort(field: string | RootFieldDto, sorting: Sorting) {
        this.filter.setOrderField(field, sorting);

        this.search();
    }

    public trackByContent(index: number, content: ContentDto): string {
        return content.id;
    }

    private updateSelectionSummary() {
        this.isAllSelected = this.contentsState.snapshot.contents.length > 0;

        this.selectionCount = 0;
        this.selectionCanDelete = true;

        const allActions = {};

        for (let content of this.contentsState.snapshot.contents.values) {
            for (let info of content.statusUpdates) {
                allActions[info.status] = info.color;
            }
        }

        for (let content of this.contentsState.snapshot.contents.values) {
            if (this.selectedItems[content.id]) {
                this.selectionCount++;

                for (let action in allActions) {
                    if (!content.statusUpdates) {
                        delete allActions[action];
                    }
                }

                if (!content.canDelete) {
                    this.selectionCanDelete = false;
                }
            } else {
                this.isAllSelected = false;
            }
        }

        this.nextStatuses = Object.keys(allActions);
    }
}

