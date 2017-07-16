/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription} from 'rxjs';

import {
    AppComponentBase,
    AppsStoreService,
    AuthService,
    DateTime,
    fadeAnimation,
    FileHelper,
    NotificationService,
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
export class DashboardPageComponent extends AppComponentBase implements OnInit, OnDestroy {
    private authenticationSubscription: Subscription;

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

    public assetsCurrent: string | null = null;
    public assetsMax: string | null = null;

    public callsCurrent: string | null = null;
    public callsMax: string | null = null;

    constructor(apps: AppsStoreService, notifications: NotificationService,
        private readonly authService: AuthService,
        private readonly usagesService: UsagesService
    ) {
        super(notifications, apps);
    }

    public ngOnDestroy() {
        this.authenticationSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.appName()
            .switchMap(app => this.usagesService.getTodayStorage(app))
            .subscribe(dto => {
                this.assetsCurrent = FileHelper.fileSize(dto.size);
                this.assetsMax = FileHelper.fileSize(dto.maxAllowed);
            });

        this.appName()
            .switchMap(app => this.usagesService.getMonthCalls(app))
            .subscribe(dto => {
                this.callsCurrent = formatCalls(dto.count);
                this.callsMax = formatCalls(dto.maxAllowed);
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
                            backgroundColor: 'rgba(61, 135, 213, 0.6)',
                            borderColor: 'rgba(61, 135, 213, 1)',
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
                            backgroundColor: 'rgba(61, 135, 213, 0.6)',
                            borderColor: 'rgba(61, 135, 213, 1)',
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
                            backgroundColor: 'rgba(61, 135, 213, 0.6)',
                            borderColor: 'rgba(61, 135, 213, 1)',
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
                            backgroundColor: 'rgba(61, 135, 213, 0.6)',
                            borderColor: 'rgba(61, 135, 213, 1)',
                            borderWidth: 1,
                            data: dtos.map(x => x.averageMs)
                        }
                    ]
                };
            });

        this.authenticationSubscription =
            this.authService.isAuthenticated.subscribe(() => {
                const user = this.authService.user;

                if (user) {
                    this.profileDisplayName = user.displayName;
                }
            });
    }

    public showForum() {
        _urq.push(['Feedback_Open']);
    }
}

function formatCalls(count: number): string | null {
    if (count > 1000) {
        count = count / 1000;

        if (count < 10) {
            count = Math.round(count * 10) / 10;
        } else {
            count = Math.round(count);
        }

        return count + 'k';
    } else if (count < 0) {
        return null;
    } else {
        return count.toString();
    }
}

function createLabels(dtos: { date: DateTime }[]): string[] {
    return dtos.map(d => d.date.toStringFormat('M-DD'));
}

