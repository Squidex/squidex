/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterEveryRender, Component, effect, NgZone, Renderer2, signal, viewChild } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Gridster, GridsterConfig, GridsterItem, GridsterItemConfig, GridType } from 'angular-gridster2';
import { ApiCallsCardComponent, ApiCallsSummaryCardComponent, ApiPerformanceCardComponent, ApiTrafficCardComponent, ApiTrafficSummaryCardComponent, AssetUploadsCountCardComponent, AssetUploadsSizeCardComponent, AssetUploadsSizeSummaryCardComponent, AuthService, DateTime, defined, IFrameCardComponent, LocalStoreService, MarkdownDirective, RandomCatCardComponent, RandomDogCardComponent, Settings, SupportCardComponent, switchSafe, TeamsState, TitleComponent, TranslatePipe, UsagesService } from '@app/shared';
import { AppsCardComponent } from './cards/apps-card.component';

@Component({
    selector: 'sqx-dashboard-page',
    styleUrls: ['./dashboard-page.component.scss'],
    templateUrl: './dashboard-page.component.html',
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
    private readonly grid = viewChild<Gridster>('grid');

    public readonly selectedTeam;

    public readonly isStacked = signal(this.localStore.getBoolean(Settings.Local.DASHBOARD_CHART_STACKED));
    public readonly isScrolled = signal(false);

    public readonly storageCurrent;
    public readonly storageUsage;

    public readonly callsUsage;

    public readonly gridConfig = signal<GridsterItemConfig[]>(DEFAULT_CONFIG);
    public readonly gridOptions = DEFAULT_OPTIONS;
    public readonly gridDefault = DEFAULT_CONFIG;
    public readonly gridOptionals = OPTIONAL_CARDS;

    public readonly extendedHeight = signal('');

    public readonly user = this.authState.user?.displayName;

    constructor(teamsState: TeamsState, usagesService: UsagesService, renderer: Renderer2, zone: NgZone,
        private readonly authState: AuthService,
        private readonly localStore: LocalStoreService,
    ) {
        const dateTo = DateTime.today().toStringFormat('yyyy-MM-dd');
        const dateFrom = DateTime.today().addDays(-20).toStringFormat('yyyy-MM-dd');

        const selectedTeam = teamsState.selectedTeam.pipe(defined());

        this.selectedTeam = toSignal(selectedTeam);

        this.storageCurrent = toSignal(
            selectedTeam.pipe(switchSafe(team => usagesService.getTodayStorageForTeam(team.id))));

        this.storageUsage = toSignal(
            selectedTeam.pipe(switchSafe(team => usagesService.getStorageUsagesForTeam(team.id, dateFrom, dateTo))));

        this.callsUsage = toSignal(
            selectedTeam.pipe(switchSafe(team => usagesService.getCallsUsagesForTeam(team.id, dateFrom, dateTo))));

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

    public changeIsStacked(value: boolean) {
        this.localStore.setBoolean(Settings.Local.DASHBOARD_CHART_STACKED, value);

        this.isStacked.set(value);
    }

    public changeConfig(config: GridsterItemConfig[]) {
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
