/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Observable, Subscription } from 'rxjs';

import {
    AppContext,
    AppDto,
    DateTime,
    fadeAnimation,
    formatHistoryMessage,
    HistoryEventDto,
    HistoryService,
    UsagesService,
    UsersProviderService
} from 'shared';

declare var _urq: any;

@Component({
    selector: 'sqx-dashboard-page',
    styleUrls: ['./dashboard-page.component.scss'],
    templateUrl: './dashboard-page.component.html',
    providers: [
        AppContext
    ],
    animations: [
        fadeAnimation
    ]
})
export class DashboardPageComponent implements OnDestroy, OnInit {
    private subscriptions: Subscription[] = [];

    public profileDisplayName = '';

    public chartStorageCount: any;
    public chartStorageSize: any;
    public chartCallsCount: any;
    public chartCallsPerformance: any;

    public app = this.ctx.appChanges.filter(x => !!x).map(x => <AppDto>x);

    public chartOptions = {
        responsive: true,
        scales: {
            xAxes: [
                {
                    display: true
                }
            ],
            yAxes: [
                {
                    ticks: {
                        beginAtZero: true
                    }
                }
            ]
        },
        maintainAspectRatio: false
    };

    public history: HistoryEventDto[] = [];

    public assetsCurrent = 0;
    public assetsMax = 0;

    public callsCurrent = 0;
    public callsMax = 0;

    constructor(public readonly ctx: AppContext,
        private readonly historyService: HistoryService,
        private readonly users: UsersProviderService,
        private readonly usagesService: UsagesService
    ) {
    }

    public ngOnDestroy() {
        for (let subscription of this.subscriptions) {
            subscription.unsubscribe();
        }

        this.subscriptions = [];
    }

    public ngOnInit() {
        this.subscriptions.push(
            this.app
                .switchMap(app => this.usagesService.getTodayStorage(app.name))
                .subscribe(dto => {
                    this.assetsCurrent = dto.size;
                    this.assetsMax = dto.maxAllowed;
                }));

        this.subscriptions.push(
            this.app
                .switchMap(app => this.usagesService.getMonthCalls(app.name))
                .subscribe(dto => {
                    this.callsCurrent = dto.count;
                    this.callsMax = dto.maxAllowed;
                }));

        this.subscriptions.push(
            this.app
                .switchMap(app => this.historyService.getHistory(app.name, ''))
                .subscribe(dto => {
                    this.history = dto;
                }));

        this.subscriptions.push(
            this.app
                .switchMap(app => this.usagesService.getStorageUsages(app.name, DateTime.today().addDays(-20), DateTime.today()))
                .subscribe(dtos => {
                    this.chartStorageCount = {
                        labels: createLabels(dtos),
                        datasets: [
                            {
                                label: 'Number of Assets',
                                lineTension: 0,
                                fill: false,
                                backgroundColor: 'rgba(51, 137, 213, 0.6)',
                                borderColor: 'rgba(51, 137, 213, 1)',
                                borderWidth: 1,
                                data: dtos.map(x => x.count)
                            }
                        ]
                    };

                    this.chartStorageSize = {
                        labels: createLabels(dtos),
                        datasets: [
                            {
                                label: 'Size of Assets (MB)',
                                lineTension: 0,
                                fill: false,
                                backgroundColor: 'rgba(51, 137, 213, 0.6)',
                                borderColor: 'rgba(51, 137, 213, 1)',
                                borderWidth: 1,
                                data: dtos.map(x => Math.round(10 * (x.size / (1024 * 1024))) / 10)
                            }
                        ]
                    };
                }));

        this.subscriptions.push(
            this.app
                .switchMap(app => this.usagesService.getCallsUsages(app.name, DateTime.today().addDays(-20), DateTime.today()))
                .subscribe(dtos => {
                    this.chartCallsCount = {
                        labels: createLabels(dtos),
                        datasets: [
                            {
                                label: 'Number of API Calls',
                                backgroundColor: 'rgba(51, 137, 213, 0.6)',
                                borderColor: 'rgba(51, 137, 213, 1)',
                                borderWidth: 1,
                                data: dtos.map(x => x.count)
                            }
                        ]
                    };

                    this.chartCallsPerformance = {
                        labels: createLabels(dtos),
                        datasets: [
                            {
                                label: 'API Performance (Milliseconds)',
                                backgroundColor: 'rgba(51, 137, 213, 0.6)',
                                borderColor: 'rgba(51, 137, 213, 1)',
                                borderWidth: 1,
                                data: dtos.map(x => x.averageMs)
                            }
                        ]
                    };
                }));
    }

    public format(message: string): Observable<string> {
        return formatHistoryMessage(message, this.users);
    }

    public showForum() {
        _urq.push(['Feedback_Open']);
    }
}

function createLabels(dtos: { date: DateTime }[]): string[] {
    return dtos.map(d => d.date.toStringFormat('M-DD'));
}

