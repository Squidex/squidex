/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { AfterViewInit, Component, NgZone, OnInit, Renderer2, ViewChild } from '@angular/core';
import { AppsState, AuthService, CallsUsageDto, CurrentStorageDto, DateTime, fadeAnimation, LocalStoreService, ResourceOwner, StorageUsagePerDateDto, UsagesService } from '@app/shared';
import { GridsterComponent, GridsterConfig, GridsterItem, GridType } from 'angular-gridster2';
import { switchMap } from 'rxjs/operators';

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
    public gridOptions = DEFAULT_OPTIONS;

    public isScrolled = false;

    constructor(
        public readonly appsState: AppsState,
        public readonly authState: AuthService,
        private readonly localStore: LocalStoreService,
        private readonly renderer: Renderer2,
        private readonly usagesService: UsagesService,
        private readonly zone: NgZone
    ) {
        super();

        this.isStacked = localStore.getBoolean('dashboard.charts.stacked');
    }

    public ngOnInit() {
        const dateTo = DateTime.today().toStringFormat('yyyy-MM-dd');
        const dateFrom = DateTime.today().addDays(-20).toStringFormat('yyyy-MM-dd');

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

    public changeIsStacked(value: boolean) {
        this.localStore.setBoolean('dashboard.charts.stacked', value);

        this.isStacked = value;
    }

    public changeConfig(config: GridsterItem[]) {
        this.gridConfig = config;

        this.grid.updateGrid();
    }
}

const DEFAULT_OPTIONS: GridsterConfig = {
    displayGrid: 'onDrag&Resize',
    draggable: {
        enabled: true
    },
    fixedColWidth: 254,
    fixedRowHeight: 254,
    gridType: GridType.Fixed,
    maxItemCols: 3,
    maxItemRows: 2,
    outerMargin: true,
    outerMarginBottom: 16,
    outerMarginLeft: 16,
    outerMarginRight: 16,
    outerMarginTop: 120,
    resizable: {
        enabled: true
    }
};