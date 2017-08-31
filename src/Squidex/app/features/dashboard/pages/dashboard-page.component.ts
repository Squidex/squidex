/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';

import {
    AppComponentBase,
    AppsStoreService,
    AuthService,
    DateTime,
    DialogService,
    fadeAnimation,
    UsagesService
} from 'shared';

declare var _urq: any;

@Component({
    selector: 'sqx-dashboard-page',
    styleUrls: ['./dashboard-page.component.scss'],
    templateUrl: './dashboard-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class DashboardPageComponent extends AppComponentBase implements OnInit {
    public profileDisplayName = '';

    public chartStorageCount: any;
    public chartStorageSize: any;
    public chartCallsCount: any;
    public chartCallsPerformance: any;

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

    public assetsCurrent = 0;
    public assetsMax = 0;

    public callsCurrent = 0;
    public callsMax = 0;

    constructor(apps: AppsStoreService, dialogs: DialogService,
        private readonly authService: AuthService,
        private readonly usagesService: UsagesService
    ) {
        super(dialogs, apps);
    }

    public ngOnInit() {
        this.appName()
            .switchMap(app => this.usagesService.getTodayStorage(app))
            .subscribe(dto => {
                this.assetsCurrent = dto.size;
                this.assetsMax = dto.maxAllowed;
            });

        this.appName()
            .switchMap(app => this.usagesService.getMonthCalls(app))
            .subscribe(dto => {
                this.callsCurrent = dto.count;
                this.callsMax = dto.maxAllowed;
            });

        this.appName()
            .switchMap(app => this.usagesService.getStorageUsages(app, DateTime.today().addDays(-20), DateTime.today()))
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
            });

        this.appName()
            .switchMap(app => this.usagesService.getCallsUsages(app, DateTime.today().addDays(-20), DateTime.today()))
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
            });

        this.profileDisplayName = this.authService.user.displayName;
    }

    public showForum() {
        _urq.push(['Feedback_Open']);
    }
}

function createLabels(dtos: { date: DateTime }[]): string[] {
    return dtos.map(d => d.date.toStringFormat('M-DD'));
}

