/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: max-line-length

import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AppLanguageDto, AppsState, ContentDto, ContentsState, ContributorsState, fadeAnimation, LanguagesState, ModalModel, Queries, Query, QueryModel, queryModelFromSchema, ResourceOwner, Router2State, SchemaDetailsDto, SchemasState, TableFields, TempService, UIState } from '@app/shared';
import { combineLatest } from 'rxjs';
import { distinctUntilChanged, onErrorResumeNext, switchMap, tap } from 'rxjs/operators';
import { DueTimeSelectorComponent } from './../../shared/due-time-selector.component';

@Component({
    selector: 'sqx-contents-page',
    styleUrls: ['./contents-page.component.scss'],
    templateUrl: './contents-page.component.html',
    providers: [
        Router2State
    ],
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
        public readonly contentsRoute: Router2State,
        public readonly contentsState: ContentsState,
        private readonly appsState: AppsState,
        private readonly contributorsState: ContributorsState,
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
        if (this.appsState.snapshot.selectedApp?.canReadContributors) {
            this.contributorsState.loadIfNotLoaded();
        }

        this.own(
            combineLatest([
                this.schemasState.selectedSchema,
                this.languagesState.languages,
                this.contentsState.statuses
            ]).subscribe(([schema, languages, statuses]) => {
                this.queryModel = queryModelFromSchema(schema, languages.map(x => x.language), statuses);
            }));

        this.own(
            this.route.params.pipe(
                    switchMap(() => this.schemasState.selectedSchema), distinctUntilChanged())
                .subscribe(schema => {
                    this.resetSelection();

                    this.schema = schema;

                    this.updateQueries();
                    this.updateTable();

                    this.contentsState.loadAndListen(this.contentsRoute);
                }));

        this.own(
            this.contentsState.contents
                .subscribe(() => {
                    this.updateSelectionSummary();
                }));

        this.own(
            this.languagesState.languagesDtos
                .subscribe(languages => {
                    this.languages = languages;
                    this.language = this.languages.find(x => x.isMaster)!;
                    this.languageMaster = this.language;
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

    private selectItems(predicate?: (content: ContentDto) => boolean) {
        return this.contentsState.snapshot.contents.filter(c => this.selectedItems[c.id] && (!predicate || predicate(c)));
    }

    public isItemSelected(content: ContentDto): boolean {
        return this.selectedItems[content.id] === true;
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
            for (const content of this.contentsState.snapshot.contents) {
                this.selectedItems[content.id] = true;
            }
        }

        this.updateSelectionSummary();
    }

    public trackByContent(_index: number, content: ContentDto): string {
        return content.id;
    }

    private updateSelectionSummary() {
        this.selectedAll = this.contentsState.snapshot.contents.length > 0;
        this.selectionCount = 0;
        this.selectionCanDelete = true;
        this.nextStatuses = {};

        for (const content of this.contentsState.snapshot.contents) {
            for (const info of content.statusUpdates) {
                this.nextStatuses[info.status] = info.color;
            }
        }

        for (const content of this.contentsState.snapshot.contents) {
            if (this.selectedItems[content.id]) {
                this.selectionCount++;

                for (const action in this.nextStatuses) {
                    if (this.nextStatuses.hasOwnProperty(action)) {
                        if (!content.statusUpdates.find(x => x.status === action)) {
                            delete this.nextStatuses[action];
                        }
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
}