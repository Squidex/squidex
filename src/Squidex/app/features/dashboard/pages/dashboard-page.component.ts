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
    ResourceOwner,
    UsagesService
} from '@app/shared';

const COLORS = [
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
    public profileDisplayName = '';

    public chartStorageCount: any;
    public chartStorageSize: any;
    public chartCallsCount: any;
    public chartCallsPerformance: any;

    public isPerformanceStacked = false;

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

    public history: HistoryEventDto[] = [];

    public assetsCurrent = 0;
    public assetsMax = 0;

    public callsCurrent = 0;
    public callsMax = 0;

    constructor(
        public readonly appsState: AppsState,
        public readonly authState: AuthService,
        private readonly historyService: HistoryService,
        private readonly usagesService: UsagesService
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.appsState.selectedApp.pipe(
                    switchMap(app => this.usagesService.getTodayStorage(app.name)))
                .subscribe(dto => {
                    this.assetsCurrent = dto.size;
                    this.assetsMax = dto.maxAllowed;
                }));

        this.own(
            this.appsState.selectedApp.pipe(
                    switchMap(app => this.usagesService.getMonthCalls(app.name)))
                .subscribe(dto => {
                    this.callsCurrent = dto.count;
                    this.callsMax = dto.maxAllowed;
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
                                data: dtos.map(x => x.count)
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
                                data: dtos.map(x => Math.round(100 * (x.size / (1024 * 1024))) / 100)
                            }
                        ]
                    };
                }));

        this.own(
            this.appsState.selectedApp.pipe(
                    switchMap(app => this.usagesService.getCallsUsages(app.name, DateTime.today().addDays(-20), DateTime.today())))
                .subscribe(dtos => {
                    const labels = createLabelsFromSet(dtos);

                    this.chartCallsCount = {
                        labels,
                        datasets: Object.keys(dtos).map((k, i) => (
                            {
                                label: label(k),
                                backgroundColor: `rgba(${COLORS[i]}, 0.6)`,
                                borderColor: `rgba(${COLORS[i]}, 1)`,
                                borderWidth: 1,
                                data: dtos[k].map(x => x.count)
                            }))
                    };

                    this.chartCallsPerformance = {
                        labels,
                        datasets: Object.keys(dtos).map((k, i) => (
                            {
                                label: label(k),
                                backgroundColor: `rgba(${COLORS[i]}, 0.6)`,
                                borderColor: `rgba(${COLORS[i]}, 1)`,
                                borderWidth: 1,
                                data: dtos[k].map(x => x.averageMs)
                            }))
                    };
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

function createLabels(dtos: { date: DateTime }[]): string[] {
    return dtos.map(d => d.date.toStringFormat('M-DD'));
}

function createLabelsFromSet(dtos: { [category: string]: { date: DateTime }[] }): string[] {
    return createLabels(dtos[Object.keys(dtos)[0]]);
}