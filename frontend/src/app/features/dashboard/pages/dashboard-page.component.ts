/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterEveryRender, Component, effect, NgZone, Renderer2, signal, viewChild } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { Gridster, GridsterConfig, GridsterItem, GridsterItemConfig, GridType } from 'angular-gridster2';
import { ApiCallsCardComponent, ApiCallsSummaryCardComponent, ApiPerformanceCardComponent, ApiTrafficCardComponent, ApiTrafficSummaryCardComponent, AppsState, AssetUploadsCountCardComponent, AssetUploadsSizeCardComponent, AssetUploadsSizeSummaryCardComponent, AuthService, DateTime, defined, IFrameCardComponent, LocalStoreService, MarkdownDirective, RandomCatCardComponent, RandomDogCardComponent, Settings, SupportCardComponent, switchSafe, TitleComponent, TourStepDirective, TranslatePipe, UsagesService } from '@app/shared';
import { ApiCardComponent } from './cards/api-card.component';
import { ContentSummaryCardComponent } from './cards/content-summary-card.component';
import { GithubCardComponent } from './cards/github-card.component';
import { HistoryCardComponent } from './cards/history-card.component';
import { SchemaCardComponent } from './cards/schema-card.component';
import { DashboardConfigComponent } from './dashboard-config.component';

@Component({
    selector: 'sqx-dashboard-page',
    styleUrls: ['./dashboard-page.component.scss'],
    templateUrl: './dashboard-page.component.html',
    imports: [
        ApiCallsCardComponent,
        ApiCallsSummaryCardComponent,
        ApiCardComponent,
        ApiPerformanceCardComponent,
        ApiTrafficCardComponent,
        ApiTrafficSummaryCardComponent,
        AssetUploadsCountCardComponent,
        AssetUploadsSizeCardComponent,
        AssetUploadsSizeSummaryCardComponent,
        ContentSummaryCardComponent,
        DashboardConfigComponent,
        GithubCardComponent,
        Gridster,
        GridsterItem,
        HistoryCardComponent,
        IFrameCardComponent,
        MarkdownDirective,
        RandomCatCardComponent,
        RandomDogCardComponent,
        SchemaCardComponent,
        SupportCardComponent,
        TitleComponent,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class DashboardPageComponent {
    private readonly grid = viewChild<Gridster>('grid');

    protected readonly selectedApp;

    protected readonly isStacked = signal(this.localStore.getBoolean(Settings.Local.DASHBOARD_CHART_STACKED));
    protected readonly isScrolled = signal(false);

    protected readonly storageCurrent;
    protected readonly storageUsage;

    protected readonly callsUsage;

    protected readonly gridConfig = signal<GridsterItemConfig[] | undefined>(undefined);
    protected readonly gridConfigAvailable = [...DEFAULT_CELLS, ...ADDITIONAL_CELLS];
    protected readonly gridConfigDefaults = DEFAULT_CELLS;
    protected readonly gridOptions = DEFAULT_OPTIONS;

    protected readonly extendedHeight = signal('');

    protected readonly user = this.authState.user?.displayName;

    constructor(
        private readonly appsState: AppsState,
        private readonly authState: AuthService,
        private readonly localStore: LocalStoreService,
        renderer: Renderer2,
        usagesService: UsagesService,
        zone: NgZone,
    ) {
        const dateTo = DateTime.today().toStringFormat('yyyy-MM-dd');
        const dateFrom = DateTime.today().addDays(-20).toStringFormat('yyyy-MM-dd');

        this.selectedApp = this.appsState.selectedApp;

        const selectedApp = toObservable(this.appsState.selectedApp).pipe(defined());

        this.storageCurrent = toSignal(
            selectedApp.pipe(switchSafe(app => usagesService.getTodayStorage(app.name))));

        this.storageUsage = toSignal(
            selectedApp.pipe(switchSafe(app => usagesService.getStorageUsages(app.name, dateFrom, dateTo))));

        this.callsUsage = toSignal(
            selectedApp.pipe(switchSafe(app => usagesService.getCallsUsages(app.name, dateFrom, dateTo))));

        effect(onCleanup => {
            const grid = this.grid();
            if (!grid) {
                return;
            }

            const gridElement = grid.el;

            const unlisten = zone.runOutsideAngular(() => {
                return renderer.listen(gridElement, 'scroll', () => {
                    const isScrolled = gridElement.scrollTop > 0;

                    if (isScrolled !== this.isScrolled()) {
                        zone.run(() => {
                            this.isScrolled.set(isScrolled);
                        });
                    }
                });
            });

            onCleanup(unlisten);
        });

        afterEveryRender({
            write: () => {
                const grid = this.grid();
                if (!grid) {
                    return;
                }

                this.extendedHeight.set(`${grid.gridRows.length * grid.curRowHeight - (this.gridOptions.margin || 0)}px`);
            },
        });
    }

    protected changeIsStacked(value: boolean) {
        this.localStore.setBoolean(Settings.Local.DASHBOARD_CHART_STACKED, value);

        this.isStacked.set(value);
    }

    protected changeConfig(config: GridsterItemConfig[]) {
        this.gridConfig.set(config);

        this.grid()?.updateGrid();
    }
}

const DEFAULT_CELLS: GridsterItemConfig[] = [
    // Row 1
    { cols: 1, rows: 1, x: 0, y: 0, name: 'i18n:dashboard.schemasCard', type: 'schemas' },
    { cols: 1, rows: 1, x: 1, y: 0, name: 'i18n:dashboard.apiDocumentationCard', type: 'api' },
    { cols: 1, rows: 1, x: 2, y: 0, name: 'i18n:dashboard.supportCard', type: 'support' },
    { cols: 1, rows: 1, x: 3, y: 0, name: 'i18n:dashboard.githubCard', type: 'github' },

    // Row 2
    { cols: 2, rows: 1, x: 0, y: 1, name: 'i18n:dashboard.apiCallsChart', type: 'api-calls' },
    { cols: 2, rows: 1, x: 2, y: 1, name: 'i18n:dashboard.apiPerformanceChart', type: 'api-performance' },

    // Row 3
    { cols: 1, rows: 1, x: 0, y: 2, name: 'i18n:dashboard.apiCallsSummaryCard', type: 'api-calls-summary' },
    { cols: 2, rows: 1, x: 1, y: 2, name: 'i18n:dashboard.assetUpdloadsCountChart', type: 'asset-uploads-count' },
    { cols: 1, rows: 1, x: 2, y: 2, name: 'i18n:dashboard.assetUploadsSizeChart', type: 'asset-uploads-size-summary' },

    // Row 4
    { cols: 2, rows: 1, x: 0, y: 3, name: 'i18n:dashboard.assetTotalSize', type: 'asset-uploads-size' },
    { cols: 2, rows: 1, x: 2, y: 3, name: 'i18n:dashboard.trafficChart', type: 'api-traffic' },

    // Row 5
    { cols: 1, rows: 1, x: 0, y: 4, name: 'i18n:dashboard.trafficSummaryCard', type: 'api-traffic-summary' },
    { cols: 2, rows: 1, x: 1, y: 4, name: 'i18n:dashboard.historyCard', type: 'history' },
];

const ADDITIONAL_CELLS = [
    { cols: 1, rows: 1, x: 0, y: 0, name: 'i18n:dashboard.randomCatCard', type: 'random-cat' },
    { cols: 1, rows: 1, x: 0, y: 0, name: 'i18n:dashboard.randomDogCard', type: 'random-dog' },
    { cols: 1, rows: 1, x: 0, y: 0, name: 'i18n:dashboard.contentSummaryCard', type: 'content-summary' },
];

const DEFAULT_OPTIONS: GridsterConfig = {
    displayGrid: 'onDrag&Resize',
    draggable: {
        enabled: true,
    },
    fixedColWidth: 254,
    fixedRowHeight: 254,
    gridType: GridType.Fixed,
    margin: 10,
    maxItemCols: 3,
    maxItemRows: 2,
    mobileBreakpoint: 0,
    outerMargin: true,
    outerMarginBottom: 16,
    outerMarginLeft: 16,
    outerMarginRight: 16,
    outerMarginTop: 120,
    resizable: {
        enabled: true,
    },
};
