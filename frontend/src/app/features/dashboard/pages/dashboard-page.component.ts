/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterEveryRender, Component, effect, inject, NgZone, Renderer2, signal, viewChild, ChangeDetectionStrategy } from '@angular/core';
import { Gridster, GridsterConfig, GridsterItem, GridsterItemConfig, GridType } from 'angular-gridster2';
import { ApiCallsCardComponent, ApiCallsSummaryCardComponent, ApiPerformanceCardComponent, ApiTrafficCardComponent, ApiTrafficSummaryCardComponent, AppsState, AssetUploadsCountCardComponent, AssetUploadsSizeCardComponent, AssetUploadsSizeSummaryCardComponent, AuthService, DateTime, IFrameCardComponent, LocalStoreService, MarkdownDirective, RandomCatCardComponent, RandomDogCardComponent, safeResource, Settings, SupportCardComponent, TitleComponent, TourStepDirective, TranslatePipe, UsagesService } from '@app/shared';
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
    changeDetection: ChangeDetectionStrategy.OnPush,
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
    private readonly appsState = inject(AppsState);
    private readonly authState = inject(AuthService);
    private readonly localStore = inject(LocalStoreService);
    private readonly renderer = inject(Renderer2);
    private readonly usageDateFrom = DateTime.today().addDays(-20).toStringFormat('yyyy-MM-dd');
    private readonly usageDateTo = DateTime.today().toStringFormat('yyyy-MM-dd');
    private readonly usagesService = inject(UsagesService);
    private readonly zone = inject(NgZone);

    protected readonly grid = viewChild<Gridster>('grid');

    protected readonly isScrolled = signal(false);

    protected readonly gridConfig = signal<GridsterItemConfig[] | undefined>(undefined);

    protected readonly extendedHeight = signal('');

    protected readonly isStacked = signal(this.localStore.getBoolean(Settings.Local.DASHBOARD_CHART_STACKED));

    protected readonly selectedApp = this.appsState.selectedApp;

    protected readonly storageCurrent = safeResource({
        params: () => this.selectedApp()?.name,
        stream: ({ params: appName }) => this.usagesService.getTodayStorage(appName),
        defaultValue: undefined,
    });

    protected readonly storageUsage = safeResource({
        params: () => this.selectedApp()?.name,
        stream: ({ params: appName }) => this.usagesService.getStorageUsages(appName, this.usageDateFrom, this.usageDateTo),
        defaultValue: undefined,
    });

    protected readonly callsUsage = safeResource({
        params: () => this.selectedApp()?.name,
        stream: ({ params: appName }) => this.usagesService.getCallsUsages(appName, this.usageDateFrom, this.usageDateTo),
        defaultValue: undefined,
    });

    protected readonly gridConfigAvailable = [...DEFAULT_CELLS, ...ADDITIONAL_CELLS];

    protected readonly gridConfigDefaults = DEFAULT_CELLS;

    protected readonly gridOptions = DEFAULT_OPTIONS;

    protected readonly user = this.authState.user?.displayName;

    constructor() {
        effect(onCleanup => {
            const grid = this.grid();
            if (!grid) {
                return;
            }

            const gridElement = grid.el;

            const unlisten = this.zone.runOutsideAngular(() => {
                return this.renderer.listen(gridElement, 'scroll', () => {
                    const isScrolled = gridElement.scrollTop > 0;

                    if (isScrolled !== this.isScrolled()) {
                        this.zone.run(() => {
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
        this.isStacked.set(value);

        this.localStore.setBoolean(Settings.Local.DASHBOARD_CHART_STACKED, value);
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
