/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @typescript-eslint/no-unnecessary-boolean-literal-compare */

import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { distinctUntilChanged, map, switchMap, take, tap } from 'rxjs/operators';
import { AppLanguageDto, AppsState, ContentDto, ContentsState, contentsTranslationStatus, ContributorsState, defined, LanguagesState, LocalStoreService, ModalModel, Queries, Query, QuerySynchronizer, ResourceOwner, Router2State, SchemaDto, SchemasService, SchemasState, Settings, switchSafe, TableSettings, TempService, TranslationStatus, UIState } from '@app/shared';
import { DueTimeSelectorComponent } from './../../shared/due-time-selector.component';

@Component({
    selector: 'sqx-contents-page',
    styleUrls: ['./contents-page.component.scss'],
    templateUrl: './contents-page.component.html',
    providers: [
        Router2State,
    ],
})
export class ContentsPageComponent extends ResourceOwner implements OnInit {
    @ViewChild('dueTimeSelector', { static: false })
    public dueTimeSelector!: DueTimeSelectorComponent;

    public schema!: SchemaDto;

    public tableSettings!: TableSettings;
    public tableViewModal = new ModalModel();

    public searchModal = new ModalModel();

    public selectedItems: { [id: string]: boolean } = {};
    public selectedAll = false;
    public selectionCount = 0;
    public selectionCanDelete = false;
    public selectionStatuses: { [name: string]: string } = {};

    public language!: AppLanguageDto;
    public languages!: ReadonlyArray<AppLanguageDto>;

    public translationStatus?: TranslationStatus;

    public get disableScheduler() {
        return this.appsState.snapshot.selectedSettings?.hideScheduler === true;
    }

    public queryModel =
        this.schemasState.selectedSchema.pipe(defined(), map(x => x.name), distinctUntilChanged(),
            switchMap(x => this.schemasService.getFilters(this.appsState.appName, x)));

    public queries =
        this.schemasState.selectedSchema.pipe(defined(), map(x => x.name), distinctUntilChanged(),
            map(x => new Queries(this.uiState, `schemas.${x}`)));

    constructor(
        public readonly contentsRoute: Router2State,
        public readonly contentsState: ContentsState,
        public readonly languagesState: LanguagesState,
        private readonly appsState: AppsState,
        private readonly contributorsState: ContributorsState,
        private readonly localStore: LocalStoreService,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly schemasState: SchemasState,
        private readonly schemasService: SchemasService,
        private readonly tempService: TempService,
        private readonly uiState: UIState,
    ) {
        super();
    }

    public ngOnInit() {
        if (this.appsState.snapshot.selectedApp?.canReadContributors) {
            this.contributorsState.loadIfNotLoaded();
        }

        this.own(
            this.languagesState.isoMasterLanguage
                .subscribe(language => {
                    this.language = language;
                }));

        this.own(
            this.languagesState.isoLanguages
                .subscribe(languages => {
                    this.languages = languages;
                }));

        this.own(
            getSchemaName(this.route).pipe(switchMap(() => this.schemasState.selectedSchema.pipe(defined(), take(1))))
                .subscribe(schema => {
                    this.resetSelection();

                    this.schema = schema;

                    this.tableSettings = new TableSettings(this.uiState, schema);

                    const initial =
                        this.contentsRoute.mapTo(this.contentsState)
                            .withPaging('contents', 10)
                            .withSynchronizer(QuerySynchronizer.INSTANCE)
                            .getInitial();

                    this.contentsState.load(false, true, initial);
                    this.contentsRoute.listen();

                    const languageKey = this.localStore.get(this.languageKey());
                    const language = this.languages.find(x => x.iso2Code === languageKey);

                    if (language) {
                        this.language = language;
                    }
                }));

        this.own(
            this.contentsState.contents
                .subscribe(contents => {
                    this.updateSelectionSummary();

                    this.translationStatus = contentsTranslationStatus(contents.map(x => x.data), this.schema, this.languages);
                }));
    }

    public reload() {
        this.contentsState.load(true);
    }

    public reloadTotal() {
        this.contentsState.load(true, false);
    }

    public delete(content: ContentDto) {
        this.contentsState.deleteMany([content]);
    }

    public deleteSelected() {
        this.contentsState.deleteMany(this.selectItems());
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
                switchSafe(d => this.contentsState.changeManyStatus(contents, action, d)))
            .subscribe();
    }

    public clone(content: ContentDto) {
        this.tempService.put(content.data);

        this.router.navigate(['new'], { relativeTo: this.route });
    }

    public changeLanguage(language: AppLanguageDto) {
        this.language = language;

        this.localStore.set(this.languageKey(), language.iso2Code);
    }

    public search(query: Query) {
        this.contentsState.search(query);
    }

    public isItemSelected(content: ContentDto): boolean {
        return this.selectedItems[content.id] === true;
    }

    public resetSelection() {
        this.selectedItems = {};

        this.updateSelectionSummary();
    }

    public selectItem(content: ContentDto, isSelected: boolean) {
        this.selectedItems[content.id] = isSelected;

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

    private updateSelectionSummary() {
        this.selectedAll = this.contentsState.snapshot.contents.length > 0;
        this.selectionCount = 0;
        this.selectionCanDelete = true;
        this.selectionStatuses = {};

        for (const content of this.contentsState.snapshot.contents) {
            for (const info of content.statusUpdates) {
                this.selectionStatuses[info.status] = info.color;
            }
        }

        for (const content of this.contentsState.snapshot.contents) {
            if (this.selectedItems[content.id]) {
                this.selectionCount++;

                for (const action of Object.keys(this.selectionStatuses)) {
                    if (!content.statusUpdates.find(x => x.status === action)) {
                        delete this.selectionStatuses[action];
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

    public trackByContent(_index: number, content: ContentDto): string {
        return content.id;
    }

    private selectItems(predicate?: (content: ContentDto) => boolean) {
        return this.contentsState.snapshot.contents.filter(c => this.selectedItems[c.id] && (!predicate || predicate(c)));
    }

    private languageKey(): any {
        return Settings.Local.CONTENT_LANGUAGE(this.schema.id);
    }
}

function getSchemaName(route: ActivatedRoute) {
    return route.params.pipe(map(x => x['schemaName'] as string), distinctUntilChanged());
}
