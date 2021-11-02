/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @typescript-eslint/no-unnecessary-boolean-literal-compare */

import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AppLanguageDto, AppsState, ContentDto, ContentsState, ContributorsState, defined, fadeAnimation, isValidFormValue, LanguagesState, ModalModel, Queries, Query, queryModelFromSchema, QuerySynchronizer, ResourceOwner, Router2State, SchemaDto, SchemasState, switchSafe, TableFields, TempService, UIState } from '@app/shared';
import { combineLatest } from 'rxjs';
import { distinctUntilChanged, map, switchMap, take, tap } from 'rxjs/operators';
import { DueTimeSelectorComponent } from './../../shared/due-time-selector.component';

@Component({
    selector: 'sqx-contents-page',
    styleUrls: ['./contents-page.component.scss'],
    templateUrl: './contents-page.component.html',
    providers: [
        Router2State,
    ],
    animations: [
        fadeAnimation,
    ],
})
export class ContentsPageComponent extends ResourceOwner implements OnInit {
    @ViewChild('dueTimeSelector', { static: false })
    public dueTimeSelector: DueTimeSelectorComponent;

    public schema: SchemaDto;

    public tableView: TableFields;
    public tableViewModal = new ModalModel();

    public searchModal = new ModalModel();

    public selectedItems: { [id: string]: boolean } = {};
    public selectedAll = false;
    public selectionCount = 0;
    public selectionCanDelete = false;
    public selectionStatuses: { [name: string]: string } = {};

    public language: AppLanguageDto;
    public languages: ReadonlyArray<AppLanguageDto>;
    public languagesData: Map<string, boolean> = new Map<string, boolean>();

    public get disableScheduler() {
        return this.appsState.snapshot.selectedSettings?.hideScheduler === true;
    }

    public queryModel =
        combineLatest([
            this.schemasState.selectedSchema.pipe(defined()),
            this.languagesState.isoLanguages,
            this.contentsState.statuses,
        ]).pipe(
            map(values => queryModelFromSchema(values[0], values[1], values[2])));

    public queries =
        this.schemasState.selectedSchema.pipe(defined(),
            map(schema => new Queries(this.uiState, `schemas.${schema.name}`)));

    constructor(
        public readonly contentsRoute: Router2State,
        public readonly contentsState: ContentsState,
        public readonly languagesState: LanguagesState,
        private readonly appsState: AppsState,
        private readonly contributorsState: ContributorsState,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly schemasState: SchemasState,
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

                    this.tableView = new TableFields(this.uiState, schema);

                    const initial =
                        this.contentsRoute.mapTo(this.contentsState)
                            .withPaging('contents', 10)
                            .withSynchronizer(QuerySynchronizer.INSTANCE)
                            .getInitial();

                    this.contentsState.load(false, initial);
                    this.contentsRoute.listen();
                }));

        this.own(
            this.contentsState.contents
                .subscribe(() => {
                    this.updateSelectionSummary();
                }));

        this.own(
            combineLatest([this.schemasState.selectedSchema, this.contentsState.contents])
                .subscribe(values => this.updateLanguageDataPresent(values[0] as SchemaDto, values[1] as ContentDto[])));
    }

    public updateLanguageDataPresent(schema: SchemaDto, contents: readonly ContentDto[]): void {
        this.languagesData.clear();

        if (contents.length === 0) {
            return;
        }

        for (const language of this.languages) {
            let dataFound = this.languagesData.get(language.iso2Code) === true;

            if (!dataFound) {
                for (const field of schema.fields.filter(f => f.isLocalizable)) {
                    for (const content of contents) {
                        const hasLanguage = content.data && content.data[field.name] && Object.keys(content.data[field.name]).includes(language.iso2Code);

                        if (hasLanguage) {
                            dataFound = isValidFormValue(content.data[field.name][language.iso2Code]);
                            if (dataFound) {
                                break;
                            }
                        }
                    }

                    this.languagesData.set(language.iso2Code, dataFound);

                    if (dataFound) {
                        break;
                    }
                }
            }
        }
    }

    public reload() {
        this.contentsState.load(true);
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

                for (const action in this.selectionStatuses) {
                    if (this.selectionStatuses.hasOwnProperty(action)) {
                        if (!content.statusUpdates.find(x => x.status === action)) {
                            delete this.selectionStatuses[action];
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

    public trackByContent(_index: number, content: ContentDto): string {
        return content.id;
    }

    private selectItems(predicate?: (content: ContentDto) => boolean) {
        return this.contentsState.snapshot.contents.filter(c => this.selectedItems[c.id] && (!predicate || predicate(c)));
    }
}

function getSchemaName(route: ActivatedRoute) {
    return route.params.pipe(map(x => x['schemaName'] as string), distinctUntilChanged());
}
