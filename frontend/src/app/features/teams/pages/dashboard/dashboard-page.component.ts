/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterEveryRender, Component, effect, inject, NgZone, Renderer2, signal, viewChild, ChangeDetectionStrategy } from '@angular/core';
import { Gridster, GridsterConfig, GridsterItem, GridsterItemConfig, GridType } from 'angular-gridster2';
import { ApiCallsCardComponent, ApiCallsSummaryCardComponent, ApiPerformanceCardComponent, ApiTrafficCardComponent, ApiTrafficSummaryCardComponent, AssetUploadsCountCardComponent, AssetUploadsSizeCardComponent, AssetUploadsSizeSummaryCardComponent, AuthService, DateTime, IFrameCardComponent, LocalStoreService, MarkdownDirective, RandomCatCardComponent, RandomDogCardComponent, safeResource, Settings, SupportCardComponent, TeamsState, TitleComponent, TranslatePipe, UsagesService } from '@app/shared';
import { AppsCardComponent } from './cards/apps-card.component';

@Component({
    selector: 'sqx-dashboard-page',
    styleUrls: ['./dashboard-page.component.scss'],
    templateUrl: './dashboard-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ApiCallsCardComponent,
        ApiCallsSummaryCardComponent,
        ApiPerformanceCardComponent,
        ApiTrafficCardComponent,
        ApiTrafficSummaryCardComponent,
        AppsCardComponent,
        AssetUploadsCountCardComponent,
        AssetUploadsSizeCardComponent,
        AssetUploadsSizeSummaryCardComponent,
        Gridster,
        GridsterItem,
        IFrameCardComponent,
        MarkdownDirective,
        RandomCatCardComponent,
        RandomDogCardComponent,
        SupportCardComponent,
        TitleComponent,
        TranslatePipe,
    ],
})
export class DashboardPageComponent {
    private readonly authState = inject(AuthService);
    private readonly localStore = inject(LocalStoreService);
    private readonly renderer = inject(Renderer2);
    private readonly teamsState = inject(TeamsState);
    private readonly usageDateFrom = DateTime.today().addDays(-20).toStringFormat('yyyy-MM-dd');
    private readonly usageDateTo = DateTime.today().toStringFormat('yyyy-MM-dd');
    private readonly usagesService = inject(UsagesService);
    private readonly zone = inject(NgZone);

    protected readonly grid = viewChild<Gridster>('grid');

    protected readonly isScrolled = signal(false);

    protected readonly gridConfig = signal<GridsterItemConfig[]>(DEFAULT_CONFIG);

    protected readonly extendedHeight = signal('');

    protected readonly isStacked = signal(this.localStore.getBoolean(Settings.Local.DASHBOARD_CHART_STACKED));

    protected readonly selectedTeam = this.teamsState.selectedTeam;

    protected readonly storageCurrent = safeResource({
        params: () => this.selectedTeam()?.id,
        stream: ({ params: teamId }) => this.usagesService.getTodayStorageForTeam(teamId),
        defaultValue: undefined,
    });

    protected readonly storageUsage = safeResource({
        params: () => this.selectedTeam()?.id,
        stream: ({ params: teamId }) => this.usagesService.getStorageUsagesForTeam(teamId, this.usageDateFrom, this.usageDateTo),
        defaultValue: undefined,
    });

    protected readonly callsUsage = safeResource({
        params: () => this.selectedTeam()?.id,
        stream: ({ params: teamId }) => this.usagesService.getCallsUsagesForTeam(teamId, this.usageDateFrom, this.usageDateTo),
        defaultValue: undefined,
    });

    protected readonly gridOptions = DEFAULT_OPTIONS;

    protected readonly gridDefault = DEFAULT_CONFIG;

    protected readonly gridOptionals = OPTIONAL_CARDS;

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

const DEFAULT_CONFIG: GridsterItemConfig[] = [
    // Row 1
    { cols: 1, rows: 1, x: 0, y: 0, name: 'i18n:dashboard.trafficSummaryCard', type: 'api-traffic-summary' },
    { cols: 2, rows: 1, x: 1, y: 0, name: 'i18n:dashboard.appsCard', type: 'apps' },
    { cols: 1, rows: 1, x: 3, y: 0, name: 'i18n:dashboard.supportCard', type: 'support' },

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
];

const OPTIONAL_CARDS = [
    { cols: 1, rows: 1, x: 0, y: 0, name: 'i18n:dashboard.randomCatCard', type: 'random-cat' },
    { cols: 1, rows: 1, x: 0, y: 0, name: 'i18n:dashboard.randomDogCard', type: 'random-dog' },
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
