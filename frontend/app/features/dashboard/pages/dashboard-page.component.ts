/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { AfterViewInit, Component, OnInit, Renderer2, ViewChild } from '@angular/core';
import { AppsState, AuthService, CallsUsageDto, CurrentStorageDto, DateTime, DialogService, fadeAnimation, LocalStoreService, ModalModel, ResourceOwner, StorageUsagePerDateDto, UIState, UsagesService } from '@app/shared';
import { GridsterComponent, GridsterItem, GridType } from 'angular-gridster2';
import { switchMap } from 'rxjs/operators';

const DEFAULT_CONFIG: ReadonlyArray<GridsterItem> = [
    { cols: 1, rows: 1, x: 0, y: 0, type: 'schemas', name: 'Schema' },
    { cols: 1, rows: 1, x: 1, y: 0, type: 'api', name: 'API Documentation' },
    { cols: 1, rows: 1, x: 2, y: 0, type: 'support', name: 'Support' },
    { cols: 1, rows: 1, x: 3, y: 0, type: 'github', name: 'Github' },

    { cols: 2, rows: 1, x: 0, y: 1, type: 'api-calls', name: 'API Calls Chart' },
    { cols: 2, rows: 1, x: 2, y: 1, type: 'api-performance', name: 'API Performance Chart' },

    { cols: 1, rows: 1, x: 0, y: 2, type: 'api-calls-summary', name: 'API Calls Summary' },
    { cols: 2, rows: 1, x: 1, y: 2, type: 'asset-uploads-count', name: 'Asset Uploads Count Chart' },
    { cols: 1, rows: 1, x: 2, y: 2, type: 'asset-uploads-size-summary', name: 'Asset Uploads Size Chart' },

    { cols: 2, rows: 1, x: 0, y: 3, type: 'asset-uploads-size', name: 'Asset Total Storage Size' },
    { cols: 2, rows: 1, x: 2, y: 3, type: 'api-traffic', name: 'API Traffic Chart' },

    { cols: 2, rows: 1, x: 0, y: 4, type: 'history', name: 'History' }
];

const DEFAULT_OPTIONS = {
    displayGrid: 'onDrag&Resize',
    fixedColWidth: 254,
    fixedRowHeight: 254,
    gridType: GridType.Fixed,
    outerMargin: true,
    outerMarginBottom: 16,
    outerMarginLeft: 16,
    outerMarginRight: 16,
    outerMarginTop: 120,
    draggable: {
        enabled: true
    },
    resizable: {
        enabled: false
    }
};

@Component({
    selector: 'sqx-dashboard-page',
    styleUrls: ['./dashboard-page.component.scss'],
    templateUrl: './dashboard-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class DashboardPageComponent extends ResourceOwner implements AfterViewInit, OnInit {
    @ViewChild('grid')
    public grid: GridsterComponent;

    public isStacked: boolean;

    public storageCurrent: CurrentStorageDto;
    public storageUsage: ReadonlyArray<StorageUsagePerDateDto>;

    public callsUsage: CallsUsageDto;

    public gridConfig: GridsterItem[];
    public gridModal = new ModalModel();
    public gridOptions = DEFAULT_OPTIONS;

    public allItems = DEFAULT_CONFIG;

    public isScrolled = false;

    constructor(
        public readonly appsState: AppsState,
        public readonly authState: AuthService,
        private readonly renderer: Renderer2,
        private readonly dialogs: DialogService,
        private readonly usagesService: UsagesService,
        private readonly localStore: LocalStoreService,
        private readonly uiState: UIState
    ) {
        super();

        this.isStacked = localStore.getBoolean('dashboard.charts.stacked');
    }

    public ngOnInit() {
        const dateTo = DateTime.today().toStringFormat('yyyy-MM-dd');
        const dateFrom = DateTime.today().addDays(-20).toStringFormat('yyyy-MM-dd');

        this.own(
            this.uiState.getUser('dashboard.grid', DEFAULT_CONFIG)
                .subscribe(dto => {
                    this.gridConfig = [...dto];
                }));

        this.own(
            this.appsState.selectedApp.pipe(
                    switchMap(app => this.usagesService.getTodayStorage(app.name)))
                .subscribe(dto => {
                    this.storageCurrent = dto;
                }));

        this.own(
            this.appsState.selectedApp.pipe(
                    switchMap(app => this.usagesService.getStorageUsages(app.name, dateFrom, dateTo)))
                .subscribe(dtos => {
                    this.storageUsage = dtos;
                }));

        this.own(
            this.appsState.selectedApp.pipe(
                    switchMap(app => this.usagesService.getCallsUsages(app.name, dateFrom, dateTo)))
                .subscribe(dto => {
                    this.callsUsage = dto;
                }));
    }

    public ngAfterViewInit() {
        this.renderer.listen(this.grid.el, 'scroll', () => {
            this.isScrolled = this.grid.el.scrollTop > 0;
        });
    }

    public changeIsStacked(value: boolean) {
        this.localStore.setBoolean('dashboard.charts.stacked', value);

        this.isStacked = value;
    }

    public reset() {
        this.gridConfig = [...this.allItems];

        this.save();
    }

    public save() {
        this.uiState.set('dashboard.grid', this.gridConfig, true);

        this.dialogs.notifyInfo('Configuration saved.');
    }

    public isSelected(item: GridsterItem) {
        return this.gridConfig && this.gridConfig.find(x => x.type === item.type);
    }

    public addOrRemove(item: GridsterItem) {
        const found = this.gridConfig.find(x => x.type === item.type);

        if (found) {
            this.gridConfig.splice(this.gridConfig.indexOf(found), 1);
        } else {
            this.gridConfig.push({ ...item });
        }
    }
}