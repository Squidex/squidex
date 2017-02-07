/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component } from '@angular/core';
import { Subscription} from 'rxjs';

import {
    AppComponentBase,
    AppsStoreService,
    AuthService,
    fadeAnimation,
    NotificationService,
    UsersProviderService
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
export class DashboardPageComponent extends AppComponentBase {
    private authenticationSubscription: Subscription;

    public profileDisplayName = '';

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly auth: AuthService
    ) {
        super(apps, notifications, users);
    }

    public ngOnDestroy() {
        this.authenticationSubscription.unsubscribe();
    }

    public ngOnInit() {
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

