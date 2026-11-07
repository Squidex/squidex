/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @typescript-eslint/no-unnecessary-boolean-literal-compare */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, linkedSignal, Signal, signal, untracked, viewChild } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { distinctUntilChanged, map, shareReplay, switchMap, take, tap } from 'rxjs/operators';
import { AppLanguageDto, AppsState, ConfirmClickDirective, ContentDto, ContentListCellDirective, ContentListCellResizeDirective, ContentListHeaderComponent, ContentListWidthDirective, ContentsService, ContentsState, ContentStatusComponent, contentsTranslationStatus, ContributorsState, defined, DialogModel, DropdownMenuComponent, EntriesPipe, getTableConfig, LanguageSelectorComponent, LanguagesState, LayoutComponent, ListViewComponent, LocalStoreService, ModalDirective, ModalModel, ModalPlacementDirective, NotifoComponent, PagerComponent, Queries, Query, QuerySynchronizer, Router2State, SchemaDto, SchemasService, SchemasState, SearchFormComponent, Settings, ShortcutDirective, SidebarMenuDirective, switchSafe, SyncWidthDirective, TableSettings, TempService, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe, TranslationStatuses, UIState } from '@app/shared';
import { DueTimeSelectorComponent } from '../../shared/due-time-selector.component';
import { ContentComponent } from '../../shared/list/content.component';
import { ContentsExportDialogComponent } from './contents-export-dialog.component';
import { CustomViewEditorComponent } from './custom-view-editor.component';

@Component({
    selector: 'sqx-contents-page',
    styleUrls: ['./contents-page.component.scss'],
    templateUrl: './contents-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        Router2State,
    ],
    imports: [
        AsyncPipe,
        ConfirmClickDirective,
        ContentComponent,
        ContentListCellDirective,
        ContentListCellResizeDirective,
        ContentListHeaderComponent,
        ContentListWidthDirective,
        ContentsExportDialogComponent,
        ContentStatusComponent,
        CustomViewEditorComponent,
        DropdownMenuComponent,
        DueTimeSelectorComponent,
        EntriesPipe,
        FormsModule,
        LanguageSelectorComponent,
        LayoutComponent,
        ListViewComponent,
        ModalDirective,
        ModalPlacementDirective,
        NotifoComponent,
        PagerComponent,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        SearchFormComponent,
        ShortcutDirective,
        SidebarMenuDirective,
        SyncWidthDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class ContentsPageComponent {
    private readonly dueTimeSelector = viewChild.required<DueTimeSelectorComponent>('dueTimeSelector');
    private readonly languageMaster = toSignal(this.languagesState.isoMasterLanguage, { initialValue: null! });
    private readonly settings = toSignal(this.appsState.selectedSettings, { initialValue: this.appsState.snapshot.selectedSettings });

    public readonly schema = signal<SchemaDto>(null!);

    public readonly tableSettings: Signal<TableSettings | undefined>;
    public readonly tableViewModal = new ModalModel();

    public readonly exportDialog = new DialogModel();

    public readonly searchModal = new ModalModel();

    public readonly contents = toSignal(this.contentsState.contents, { initialValue: [] });

    public readonly selectedItems = signal<Record<string, boolean>>({});

    public readonly selection = computed(() => {
        const contents = this.contents();
        const selectedItems = this.selectedItems();

        let selectedAll = contents.length > 0;
        let selectedCount = 0;
        let selectedCanDelete = true;

        const selectedStatuses: { [name: string]: string } = {};

        for (const content of contents) {
            for (const info of content.statusUpdates) {
                selectedStatuses[info.status] = info.color;
            }
        }

        for (const content of contents) {
            if (selectedItems[content.id]) {
                selectedCount++;

                for (const action of Object.keys(selectedStatuses)) {
                    if (!content.statusUpdates.find(x => x.status === action)) {
                        delete selectedStatuses[action];
                    }
                }

                if (!content.canDelete) {
                    selectedCanDelete = false;
                }
            } else {
                selectedAll = false;
            }
        }

        return { selectedAll, selectedCount, selectedCanDelete, selectedStatuses };
    });

    public readonly language = linkedSignal<AppLanguageDto>(() => {
        return this.languageMaster()!;
    });

    public readonly languages = toSignal(this.languagesState.isoLanguages, { initialValue: [] });

    public readonly translationStatus = computed<TranslationStatuses | undefined>(() => {
        const schema = this.schema();
        if (!schema) {
            return undefined;
        }

        return contentsTranslationStatus(this.contents().map(x => x.data), schema, this.languages());
    });

    public readonly disableScheduler = computed(() => {
        return this.settings()?.hideScheduler === true;
    });

    public readonly queryModel = toSignal(
        this.schemasState.selectedSchema.pipe(defined(), map(x => x.name), distinctUntilChanged(),
            switchMap(x => this.schemasService.getFilters(this.appsState.appName, x))));

    public readonly queries = toSignal(
        this.schemasState.selectedSchema.pipe(defined(), map(x => x.name), distinctUntilChanged(),
            map(x => new Queries(this.uiState, `schemas.${x}`))));

    constructor(
        public readonly contentsRoute: Router2State,
        public readonly contentsState: ContentsState,
        public readonly languagesState: LanguagesState,
        private readonly appsState: AppsState,
        private readonly contributorsState: ContributorsState,
        private readonly contentsService: ContentsService,
        private readonly localStore: LocalStoreService,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly schemasState: SchemasState,
        private readonly schemasService: SchemasService,
        private readonly tempService: TempService,
        private readonly uiState: UIState,
    ) {
        if (this.appsState.snapshot.selectedApp?.canReadContributors) {
            this.contributorsState.loadIfNotLoaded();
        }

        const schema$ =
            getSchemaName(this.route).pipe(switchMap(() => this.schemasState.selectedSchema.pipe(defined(), take(1), shareReplay(1))));

        const tableSetting$ =
            schema$.pipe(map(schema => new TableSettings(this.uiState, schema)), shareReplay(1));

        const tableConfig$ =
            tableSetting$.pipe(switchMap(getTableConfig));

        this.tableSettings = toSignal(tableSetting$);

        effect(onCleanup => {
            const subscription = untracked(() => tableConfig$
                .subscribe(({ fieldNames, schema }) => {
                    if (this.schema()?.id !== schema.id) {
                        this.resetSelection();

                        this.schema.set(schema);

                        const initial =
                            this.contentsRoute.mapTo(this.contentsState)
                                .withPaging('contents', 10)
                                .withSynchronizer(QuerySynchronizer.INSTANCE)
                                .getInitial();

                        this.contentsState.load(false, true, { fieldNames, ...initial });
                        this.contentsRoute.listen();

                        const languageKey = this.localStore.get(this.languageKey());
                        const languageItem = this.languages().find(x => x.iso2Code === languageKey);

                        if (languageItem) {
                            this.language.set(languageItem);
                        }
                    } else {
                        this.contentsState.setFieldNames(fieldNames);
                        this.contentsState.load();
                    }
                }));

            onCleanup(() => subscription.unsubscribe());
        });
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

        this.dueTimeSelector().selectDueTime(action).pipe(
                tap(() => {
                    this.resetSelection();
                }),
                switchSafe(d => this.contentsState.changeManyStatus(contents, action, d)))
            .subscribe();
    }

    public clone(content: ContentDto) {
        if (!content) {
            return;
        }

        this.contentsService.getContent(this.contentsState.appName, this.contentsState.schemaName, content.id)
            .subscribe(currentContent => {
                this.tempService.put(currentContent.data);
                this.router.navigate(['new'], { relativeTo: this.route });
            });
    }

    public changeLanguage(language: AppLanguageDto) {
        this.language.set(language);

        this.localStore.set(this.languageKey(), language.iso2Code);
    }

    public search(query: Query) {
        this.contentsState.search(query);
    }

    public isItemSelected(content: ContentDto): boolean {
        return this.selectedItems()[content.id] === true;
    }

    public resetSelection() {
        this.selectedItems.set({});
    }

    public selectItem(content: ContentDto, isSelected: boolean) {
        this.selectedItems.update(x => ({ ...x, [content.id]: isSelected }));
    }

    public selectAll(isSelected: boolean) {
        const selectedItems: Record<string, boolean> = {};

        if (isSelected) {
            for (const content of this.contentsState.snapshot.contents) {
                selectedItems[content.id] = true;
            }
        }

        this.selectedItems.set(selectedItems);
    }

    private selectItems(predicate?: (content: ContentDto) => boolean) {
        const selectedItems = this.selectedItems();

        return this.contentsState.snapshot.contents.filter(c => selectedItems[c.id] && (!predicate || predicate(c)));
    }

    private languageKey(): any {
        return Settings.Local.CONTENT_LANGUAGE(this.schema().id);
    }
}

function getSchemaName(route: ActivatedRoute) {
    return route.params.pipe(map(x => x['schemaName'] as string), distinctUntilChanged());
}
