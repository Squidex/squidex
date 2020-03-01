/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { switchMap } from 'rxjs/operators';

import {
    AppsState,
    AuthService,
    DateTime,
    fadeAnimation,
    HistoryEventDto,
    HistoryService,
    LocalStoreService,
    ResourceOwner,
    UsagesService
} from '@app/shared';

const COLORS: ReadonlyArray<string> = [
    ' 51, 137, 213',
    '211,  50,  50',
    '131, 211,  50',
    ' 50, 211, 131',
    ' 50, 211, 211',
    ' 50, 131, 211',
    ' 50,  50, 211',
    ' 50, 211,  50',
    '131,  50, 211',
    '211,  50, 211',
    '211,  50, 131'
];

@Component({
    selector: 'sqx-dashboard-page',
    styleUrls: ['./dashboard-page.component.scss'],
    templateUrl: './dashboard-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class DashboardPageComponent extends ResourceOwner implements OnInit {
    private isStackedValue: boolean;

    public chartOptions = {
        responsive: true,
        scales: {
            xAxes: [{
                display: true,
                stacked: false
            }],
            yAxes: [{
                ticks: {
                    beginAtZero: true
                },
                stacked: false
            }]
        },
        maintainAspectRatio: false
    };

    public stackedChartOptions = {
        responsive: true,
        scales: {
            xAxes: [{
                display: true,
                stacked: true
            }],
            yAxes: [{
                ticks: {
                    beginAtZero: true
                },
                stacked: true
            }]
        },
        maintainAspectRatio: false
    };

    public history: ReadonlyArray<HistoryEventDto> = [];

    public profileDisplayName = '';

    public chartStorageCount: any;
    public chartStorageSize: any;
    public chartCallsCount: any;
    public chartCallsBytes: any;
    public chartCallsPerformance: any;

    public storageCurrent = 0;
    public storageAllowed = 0;

    public callsPerformance = 0;
    public callsCurrent = 0;
    public callsAllowed = 0;
    public callsBytes = 0;

    public get isStacked() {
        return this.isStackedValue;
    }

    public set isStacked(value: boolean) {
        this.localStore.setBoolean('dashboard.charts.stacked', value);

        this.isStackedValue = value;
    }

    constructor(
        public readonly appsState: AppsState,
        public readonly authState: AuthService,
        private readonly historyService: HistoryService,
        private readonly usagesService: UsagesService,
        private readonly localStore: LocalStoreService
    ) {
        super();

        this.isStackedValue = localStore.getBoolean('dashboard.charts.stacked');
    }

    public ngOnInit() {
        this.own(
            this.appsState.selectedApp.pipe(
                    switchMap(app => this.usagesService.getTodayStorage(app.name)))
                .subscribe(dto => {
                    this.storageCurrent = dto.size;
                    this.storageAllowed = dto.maxAllowed;
                }));

        this.own(
            this.appsState.selectedApp.pipe(
                    switchMap(app => this.historyService.getHistory(app.name, '')))
                .subscribe(dto => {
                    this.history = dto;
                }));

        this.own(
            this.appsState.selectedApp.pipe(
                    switchMap(app => this.usagesService.getStorageUsages(app.name, DateTime.today().addDays(-20), DateTime.today())))
                .subscribe(dtos => {
                    const labels = createLabels(dtos);

                    this.chartStorageCount = {
                        labels,
                        datasets: [
                            {
                                label: 'All',
                                lineTension: 0,
                                fill: false,
                                backgroundColor: `rgba(${COLORS[0]}, 0.6)`,
                                borderColor: `rgba(${COLORS[0]}, 1)`,
                                borderWidth: 1,
                                data: dtos.map(x => x.totalCount)
                            }
                        ]
                    };

                    this.chartStorageSize = {
                        labels,
                        datasets: [
                            {
                                label: 'All',
                                lineTension: 0,
                                fill: false,
                                backgroundColor: `rgba(${COLORS[0]}, 0.6)`,
                                borderColor: `rgba(${COLORS[0]}, 1)`,
                                borderWidth: 1,
                                data: dtos.map(x => Math.round(100 * (x.totalSize / (1024 * 1024))) / 100)
                            }
                        ]
                    };
                }));

        this.own(
            this.appsState.selectedApp.pipe(
                    switchMap(app => this.usagesService.getCallsUsages(app.name, DateTime.today().addDays(-20), DateTime.today())))
                .subscribe(({ details, totalBytes, totalCalls, allowedCalls, averageElapsedMs }) => {
                    const labels = createLabelsFromSet(details);

                    this.chartCallsCount = {
                        labels,
                        datasets: Object.keys(details).map((k, i) => (
                            {
                                label: label(k),
                                backgroundColor: `rgba(${COLORS[i]}, 0.6)`,
                                borderColor: `rgba(${COLORS[i]}, 1)`,
                                borderWidth: 1,
                                data: details[k].map(x => x.totalCalls)
                            }))
                    };

                    this.chartCallsBytes = {
                        labels,
                        datasets: Object.keys(details).map((k, i) => (
                            {
                                label: label(k),
                                backgroundColor: `rgba(${COLORS[i]}, 0.6)`,
                                borderColor: `rgba(${COLORS[i]}, 1)`,
                                borderWidth: 1,
                                data: details[k].map(x => Math.round(100 * (x.totalBytes / (1024 * 1024))) / 100)
                            }))
                    };

                    this.chartCallsPerformance = {
                        labels,
                        datasets: Object.keys(details).map((k, i) => (
                            {
                                label: label(k),
                                backgroundColor: `rgba(${COLORS[i]}, 0.6)`,
                                borderColor: `rgba(${COLORS[i]}, 1)`,
                                borderWidth: 1,
                                data: details[k].map(x => x.averageElapsedMs)
                            }))
                    };

                    this.callsPerformance = averageElapsedMs;
                    this.callsBytes = totalBytes;
                    this.callsCurrent = totalCalls;
                    this.callsAllowed = allowedCalls;
                }));
    }

    public downloadLog() {
        this.usagesService.getLog(this.appsState.appName)
            .subscribe(url => {
                window.open(url, '_blank');
            });
    }
}

function label(category: string) {
    return category === '*' ? 'anonymous' : category;
}

function createLabels(dtos: ReadonlyArray<{ date: DateTime }>): ReadonlyArray<string> {
    return dtos.map(d => d.date.toStringFormat('M-DD'));
}

function createLabelsFromSet(dtos: { [category: string]: ReadonlyArray<{ date: DateTime }> }): ReadonlyArray<string> {
    return createLabels(dtos[Object.keys(dtos)[0]]);
}