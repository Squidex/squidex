/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @typescript-eslint/no-unnecessary-boolean-literal-compare */

import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AppLanguageDto, AppsState, ContentDto, ContentsState, ContributorsState, defined, fadeAnimation, LanguagesState, ModalModel, Queries, Query, queryModelFromSchema, QuerySynchronizer, ResourceOwner, Router2State, SchemaDto, SchemasState, switchSafe, TableFields, TempService, UIState } from '@app/shared';
import { combineLatest } from 'rxjs';
import { distinctUntilChanged, map, switchMap, take, tap } from 'rxjs/operators';
import { DueTimeSelectorComponent } from './../../shared/due-time-selector.component';
import { ExtensionInfo } from '../../../../shared/state/plugin';
import { LocalizerService } from '../../../../framework/services/localizer.service';

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

    public extensionData: Array<ExtensionInfo> = [];

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
        private readonly localizer: LocalizerService,
    ) {
        super();
    }

    public setExtensionInfo(params: string) {
        localStorage.setItem('selectedContentsSidebarData', JSON.stringify(params));
    };

    private createExtensionData(contentEditorData: string | undefined) {
        if (contentEditorData && contentEditorData !== null) {
            const editorUrls = contentEditorData.split(',');
            for (let index = 0; index < editorUrls.length; index++) {
                const values = editorUrls[index].split('?');
                const localizeText = this.localizer.getOrKey('i18n:common.extension');
                let extensionInfo: ExtensionInfo = {
                    url: values[0],
                    icon: "icon-plugin",
                    name: `${localizeText}-${index + 1}`,
                    value: `${localizeText}-${index + 1}`,
                }
                if (values.length > 1) {
                    const params = values[1].split('&');
                    extensionInfo.name = params[0].split('=')[1];
                    extensionInfo.value = `${params[0].split('=')[1]}${index + 1}`;
                    extensionInfo.icon = params.length === 1 ? extensionInfo.icon : params[1];
                }
                this.extensionData.push(extensionInfo);
            }
        }
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
                    this.extensionData = [];
                    this.schema = schema;

                    if (this.schema && this.schema.properties) {
                        this.createExtensionData(this.schema.properties.contentsSidebarUrl);
                    }

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
