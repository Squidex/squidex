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

    public chartCount: any;
    public chartPerformance: any;
    public chartOptions = { };

    public monthlyCalls: string = null;

    constructor(apps: AppsStoreService, notifications: NotificationService,
        private readonly auth: AuthService,
        private readonly usagesService: UsagesService
    ) {
        super(notifications, apps);
    }

    public ngOnDestroy() {
        this.authenticationSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.appName()
            .switchMap(app => this.usagesService.getMonthlyCalls(app))
            .subscribe(dto => {
                if (dto.count > 1000) {
                    this.monthlyCalls = Math.round(dto.count / 1000) + 'k';
                } else {
                    this.monthlyCalls = dto.count.toString();
                }
            });

        this.appName()
            .switchMap(app => this.usagesService.getUsages(app, DateTime.today().addDays(-30), DateTime.today()))
            .subscribe(dtos => {
                const usages: any[] = dtos.map(x => { return { date: x.date.toStringFormat('L'), count: x.count, averageMs: x.averageMs }; });

                this.chartCount = {
                    labels: usages.map(x => x.date),
                    datasets: [
                        {
                            label: 'Number of API Calls',
                            backgroundColor: 'rgba(61, 135, 213, 0.6)',
                            borderColor: 'rgba(61, 135, 213, 1)',
                            borderWidth: 1,
                            data: usages.map(x => x.count)
                        }
                    ]
                };

                this.chartPerformance = {
                    labels: usages.map(x => x.date),
                    datasets: [
                        {
                            label: 'API Performance (Milliseconds)',
                            backgroundColor: 'rgba(61, 135, 213, 0.6)',
                            borderColor: 'rgba(61, 135, 213, 1)',
                            borderWidth: 1,
                            data: usages.map(x => x.averageMs)
                        }
                    ]
                };
            });

        this.authenticationSubscription =
            this.auth.isAuthenticated.subscribe(() => {
                const user = this.auth.user;

                if (user) {
                    this.profileDisplayName = user.displayName;
                }
            });
    }

    public showForum() {
        _urq.push(['Feedback_Open']);
    }
}

