/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { onErrorResumeNext, switchMap, tap } from 'rxjs/operators';

import {
    AppLanguageDto,
    ContentDto,
    ContentsState,
    fadeAnimation,
    LanguagesState,
    ModalModel,
    Queries,
    Query,
    QueryModel,
    queryModelFromSchema,
    ResourceOwner,
    SchemaDetailsDto,
    SchemasState,
    TableFields,
    TempService,
    UIState
} from '@app/shared';

import { DueTimeSelectorComponent } from './../../shared/due-time-selector.component';

@Component({
    selector: 'sqx-contents-page',
    styleUrls: ['./contents-page.component.scss'],
    templateUrl: './contents-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class ContentsPageComponent extends ResourceOwner implements OnInit {
    public schema: SchemaDetailsDto;

    public tableView: TableFields;
    public tableViewModal = new ModalModel();

    public searchModal = new ModalModel();

    public selectedItems:  { [id: string]: boolean; } = {};
    public selectedAll = false;
    public selectionCount = 0;
    public selectionCanDelete = false;

    public nextStatuses: { [name: string]: string } = {};

    public language: AppLanguageDto;
    public languageMaster: AppLanguageDto;
    public languages: ReadonlyArray<AppLanguageDto>;

    public queryModel: QueryModel;
    public queries: Queries;

    @ViewChild('dueTimeSelector', { static: false })
    public dueTimeSelector: DueTimeSelectorComponent;

    constructor(
        public readonly contentsState: ContentsState,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly languagesState: LanguagesState,
        private readonly schemasState: SchemasState,
        private readonly tempService: TempService,
        private readonly uiState: UIState
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.schemasState.selectedSchema
                .subscribe(schema => {
                    this.resetSelection();

                    this.schema = schema;

                    this.contentsState.load();

                    this.updateQueries();
                    this.updateModel();
                    this.updateTable();
                }));

        this.own(
            this.contentsState.statuses
                .subscribe(() => {
                    this.updateModel();
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
                    this.language = this.languages.find(x => x.isMaster)!;
                    this.languageMaster = this.language;

                    this.updateModel();
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

    private changeContentItems(contents: ReadonlyArray<ContentDto>, action: string) {
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

    public clone(content: ContentDto) {
        this.tempService.put(content.data);

        this.router.navigate(['new'], { relativeTo: this.route });
    }

    public search(query: Query) {
        this.contentsState.search(query);
    }

    public isItemSelected(content: ContentDto): boolean {
        return this.selectedItems[content.id] === true;
    }

    private selectItems(predicate?: (content: ContentDto) => boolean) {
        return this.contentsState.snapshot.contents.filter(c => this.selectedItems[c.id] && (!predicate || predicate(c)));
    }

    public selectLanguage(language: AppLanguageDto) {
        this.language = language;
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
            for (let content of this.contentsState.snapshot.contents) {
                this.selectedItems[content.id] = true;
            }
        }

        this.updateSelectionSummary();
    }

    public trackByContent(index: number, content: ContentDto): string {
        return content.id;
    }

    private updateSelectionSummary() {
        this.selectedAll = this.contentsState.snapshot.contents.length > 0;
        this.selectionCount = 0;
        this.selectionCanDelete = true;
        this.nextStatuses = {};

        for (let content of this.contentsState.snapshot.contents) {
            for (const info of content.statusUpdates) {
                this.nextStatuses[info.status] = info.color;
            }
        }

        for (let content of this.contentsState.snapshot.contents) {
            if (this.selectedItems[content.id]) {
                this.selectionCount++;

                for (const action in this.nextStatuses) {
                    if (!content.statusUpdates.find(x => x.status === action)) {
                        delete this.nextStatuses[action];
                    }
                }

                if (!content.canDelete) {
                    this.selectionCanDelete = false;
                }
            } else {
                this.selectedAll = false;
            }
        }
    }

    private updateQueries() {
        if (this.schema) {
            this.queries = new Queries(this.uiState, `schemas.${this.schema.name}`);
        }
    }

    private updateTable() {
        if (this.schema) {
            this.tableView = new TableFields(this.uiState, this.schema);
        }
    }

    private updateModel() {
        if (this.schema && this.languages) {
            this.queryModel = queryModelFromSchema(this.schema, this.languages, this.contentsState.snapshot.statuses);
        }
    }
}