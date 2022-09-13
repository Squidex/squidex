/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Component, NgZone, OnInit, Renderer2, ViewChild } from '@angular/core';
import { GridsterComponent, GridsterConfig, GridsterItem, GridType } from 'angular-gridster2';
import { AuthService, CallsUsageDto, CurrentStorageDto, DateTime, defined, fadeAnimation, LocalStoreService, ResourceOwner, Settings, StorageUsagePerDateDto, switchSafe, TeamsState, UsagesService } from '@app/shared';

@Component({
    selector: 'sqx-dashboard-page',
    styleUrls: ['./dashboard-page.component.scss'],
    templateUrl: './dashboard-page.component.html',
    animations: [
        fadeAnimation,
    ],
})
export class DashboardPageComponent extends ResourceOwner implements AfterViewInit, OnInit {
    @ViewChild('grid')
    public grid!: GridsterComponent;

    public selectedTeam = this.teamsState.selectedTeam.pipe(defined());

    public isStacked = false;
    public isScrolled = false;

    public storageCurrent?: CurrentStorageDto;
    public storageUsage?: ReadonlyArray<StorageUsagePerDateDto>;

    public callsUsage?: CallsUsageDto;

    public gridConfig: GridsterItem[] = DEFAULT_CONFIG;
    public gridOptions = DEFAULT_OPTIONS;
    public gridDefault = DEFAULT_CONFIG;
    public gridOptionals = OPTIONAL_CARDS;

    public extendedHeight: string = '';

    public user = this.authState.user?.displayName;

    constructor(
        private readonly authState: AuthService,
        private readonly localStore: LocalStoreService,
        private readonly renderer: Renderer2,
        private readonly teamsState: TeamsState,
        private readonly usagesService: UsagesService,
        private readonly zone: NgZone,
    ) {
        super();

        this.isStacked = localStore.getBoolean(Settings.Local.DASHBOARD_CHART_STACKED);
    }

    public ngOnInit() {
        const dateTo = DateTime.today().toStringFormat('yyyy-MM-dd');
        const dateFrom = DateTime.today().addDays(-20).toStringFormat('yyyy-MM-dd');

        this.own(
            this.selectedTeam.pipe(switchSafe(team => this.usagesService.getTodayStorageForTeam(team.id)))
                .subscribe(dto => {
                    this.storageCurrent = dto;
                }));

        this.own(
            this.selectedTeam.pipe(switchSafe(team => this.usagesService.getStorageUsagesForTeam(team.id, dateFrom, dateTo)))
                .subscribe(dtos => {
                    this.storageUsage = dtos;
                }));

        this.own(
            this.selectedTeam.pipe(switchSafe(team => this.usagesService.getCallsUsagesForTeam(team.id, dateFrom, dateTo)))
                .subscribe(dto => {
                    this.callsUsage = dto;
                }));
    }

    public ngAfterViewInit() {
        this.zone.runOutsideAngular(() => {
            const gridElement = this.grid.el;

            this.renderer.listen(gridElement, 'scroll', () => {
                const isScrolled = gridElement.scrollTop > 0;

                if (isScrolled !== this.isScrolled) {
                    this.zone.run(() => {
                        this.isScrolled = isScrolled;
                    });
                }
            });
        });
    }

    public ngAfterViewChecked() {
        this.extendedHeight = `${this.grid.gridRows.length * this.grid.curRowHeight - (this.gridOptions.margin || 0)}px`;
    }

    public changeIsStacked(value: boolean) {
        this.localStore.setBoolean(Settings.Local.DASHBOARD_CHART_STACKED, value);

        this.isStacked = value;
    }

    public changeConfig(config: GridsterItem[]) {
        this.gridConfig = config;

        this.grid?.updateGrid();
    }
}

const DEFAULT_CONFIG: GridsterItem[] = [
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
    outerMargin: true,
    outerMarginBottom: 16,
    outerMarginLeft: 16,
    outerMarginRight: 16,
    outerMarginTop: 120,
    resizable: {
        enabled: true,
    },
};
