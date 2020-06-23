/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { AppsState, AuthService, CallsUsageDto, CurrentStorageDto, DateTime, fadeAnimation, LocalStoreService, ResourceOwner, StorageUsagePerDateDto, UsagesService } from '@app/shared';
import { switchMap } from 'rxjs/operators';

@Component({
    selector: 'sqx-dashboard-page',
    styleUrls: ['./dashboard-page.component.scss'],
    templateUrl: './dashboard-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class DashboardPageComponent extends ResourceOwner implements OnInit {
    public isStacked: boolean;

    public storageUsage: ReadonlyArray<StorageUsagePerDateDto>;
    public storageCurrent: CurrentStorageDto;

    public callsUsage: CallsUsageDto;

    constructor(
        public readonly appsState: AppsState,
        public readonly authState: AuthService,
        private readonly usagesService: UsagesService,
        private readonly localStore: LocalStoreService
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

    public changeIsStacked(value: boolean) {
        this.localStore.setBoolean('dashboard.charts.stacked', value);

        this.isStacked = value;
    }
}