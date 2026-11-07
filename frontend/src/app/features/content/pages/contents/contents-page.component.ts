/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @typescript-eslint/no-unnecessary-boolean-literal-compare */

import { ChangeDetectionStrategy, Component, computed, effect, inject, linkedSignal, signal, untracked, viewChild } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { tap } from 'rxjs/operators';
import { AppLanguageDto, AppsState, ConfirmClickDirective, ContentDto, ContentListCellDirective, ContentListCellResizeDirective, ContentListHeaderComponent, ContentListWidthDirective, ContentsService, ContentsState, ContentStatusComponent, contentsTranslationStatus, ContributorsState, DialogModel, DropdownMenuComponent, EntriesPipe, getTableConfig, LanguageSelectorComponent, LanguagesState, LayoutComponent, ListViewComponent, LocalStoreService, ModalDirective, ModalModel, ModalPlacementDirective, NotifoComponent, PagerComponent, Queries, Query, QuerySynchronizer, Router2State, SchemaDto, SchemasService, SchemasState, SearchFormComponent, Settings, ShortcutDirective, SidebarMenuDirective, switchSafe, SyncWidthDirective, TableSettings, TempService, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe, TranslationStatuses, UIState } from '@app/shared';
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
    private readonly appsState = inject(AppsState);
    private readonly contentsService = inject(ContentsService);
    private readonly contributorsState = inject(ContributorsState);
    private readonly localStore = inject(LocalStoreService);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly schemasService = inject(SchemasService);
    private readonly schemasState = inject(SchemasState);
    private readonly tempService = inject(TempService);
    private readonly uiState = inject(UIState);
    private schemaLoaded?: SchemaDto;

    protected readonly contentsRoute = inject(Router2State);

    protected readonly contentsState = inject(ContentsState);

    protected readonly languagesState = inject(LanguagesState);

    protected readonly dueTimeSelector = viewChild.required<DueTimeSelectorComponent>('dueTimeSelector');

    protected readonly selectedItems = signal<Record<string, boolean>>({});

    protected readonly language = linkedSignal<AppLanguageDto>(() => {
        const languageKey = this.localStore.get(Settings.Local.CONTENT_LANGUAGE(this.schema().id));
        const languageMaster = this.languagesState.isoMasterLanguage();

        return this.languages().find(x => x.iso2Code === languageKey) || languageMaster;
    });

    protected readonly tableConfig = computed(() => {
        return getTableConfig(this.tableSettings());
    });

    protected readonly schemaName = computed(() => {
        return this.schemasState.selectedSchema()?.name;
    });

    protected readonly schema = computed(() => {
        return this.schemasState.selectedSchema()!;
    }, { equal: (lhs, rhs) => lhs?.id === rhs?.id });

    protected readonly tableSettings = computed(() => {
        const schema = this.schema();

        return untracked(() => new TableSettings(this.uiState, schema));
    });

    protected readonly contents = computed(() => {
        return this.contentsState.contents();
    });

    protected readonly selection = computed(() => {
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

    protected readonly languages = computed(() => {
        return this.languagesState.isoLanguages();
    });

    protected readonly translationStatus = computed<TranslationStatuses | undefined>(() => {
        const schema = this.schema();
        if (!schema) {
            return undefined;
        }

        return contentsTranslationStatus(this.contents().map(x => x.data), schema, this.languages());
    });

    protected readonly disableScheduler = computed(() => {
        return this.appsState.selectedSettings()?.hideScheduler === true;
    });

    protected readonly queries = computed(() => {
        const schemaName = this.schemaName();
        if (!schemaName) {
            return undefined;
        }

        return new Queries(this.uiState, `schemas.${schemaName}`);
    });

    protected readonly queryModel = rxResource({
        params: () => this.schemaName(),
        stream: ({ params: schemaName }) => this.schemasService.getFilters(this.appsState.appName, schemaName),
    });

    protected readonly tableViewModal = new ModalModel();

    protected readonly exportDialog = new DialogModel();

    public readonly searchModal = new ModalModel();

    constructor() {
        if (this.appsState.snapshot.selectedApp?.canReadContributors) {
            this.contributorsState.loadIfNotLoaded();
        }

        effect(() => {
            const { fieldNames, schema } = this.tableConfig()();

            untracked(() => this.loadContents(fieldNames, schema));
        });
    }

    private loadContents(fieldNames: string[], schema: SchemaDto) {
        if (this.schemaLoaded?.id === schema.id) {
            this.contentsState.setFieldNames(fieldNames);
            this.contentsState.load();
            return;
        }

        this.schemaLoaded = schema;
        this.resetSelection();

        const initial =
            this.contentsRoute.mapTo(this.contentsState)
                .withPaging('contents', 10)
                .withSynchronizer(QuerySynchronizer.INSTANCE)
                .getInitial();

        this.contentsState.load(false, true, { fieldNames, ...initial });
        this.contentsRoute.listen();
    }

    protected reload() {
        this.contentsState.load(true);
    }

    protected reloadTotal() {
        this.contentsState.load(true, false);
    }

    protected delete(content: ContentDto) {
        this.contentsState.deleteMany([content]);
    }

    protected deleteSelected() {
        this.contentsState.deleteMany(this.selectItems());
    }

    protected changeStatus(content: ContentDto, status: string) {
        this.changeContentItems([content], status);
    }

    protected changeSelectedStatus(status: string) {
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

    protected clone(content: ContentDto) {
        if (!content) {
            return;
        }

        this.contentsService.getContent(this.contentsState.appName, this.contentsState.schemaName, content.id)
            .subscribe(currentContent => {
                this.tempService.put(currentContent.data);
                this.router.navigate(['new'], { relativeTo: this.route });
            });
    }

    protected changeLanguage(language: AppLanguageDto) {
        this.language.set(language);

        this.localStore.set(Settings.Local.CONTENT_LANGUAGE(this.schema().id), language.iso2Code);
    }

    protected search(query: Query) {
        this.contentsState.search(query);
    }

    protected isItemSelected(content: ContentDto): boolean {
        return this.selectedItems()[content.id] === true;
    }

    private resetSelection() {
        this.selectedItems.set({});
    }

    protected selectItem(content: ContentDto, isSelected: boolean) {
        this.selectedItems.update(x => ({ ...x, [content.id]: isSelected }));
    }

    protected selectAll(isSelected: boolean) {
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
}
