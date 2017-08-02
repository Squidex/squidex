/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';

import {
    Notification,
    NotificationService
} from 'shared';

@Component({
    selector: 'sqx-internal-area',
    styleUrls: ['./internal-area.component.scss'],
    templateUrl: './internal-area.component.html'
})
export class InternalAreaComponent implements OnInit, OnDestroy {
    private notificationsSubscription: Subscription;
    private queryParamsSubscription: Subscription;

    public notifications: Notification[] = [];

    constructor(
        private readonly notificationService: NotificationService,
        private readonly route: ActivatedRoute
    ) {
    }

    public ngOnDestroy() {
        this.queryParamsSubscription.unsubscribe();
        this.notificationsSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.notificationsSubscription =
            this.notificationService.notifications.subscribe(notification => {
                this.notifications.push(notification);

                if (notification.displayTime > 0) {
                    setTimeout(() => {
                        this.close(notification);
                    }, notification.displayTime);
                }
            });

        this.queryParamsSubscription =
            this.route.queryParams.subscribe(params => {
                const successMessage = params['successMessage'];

                if (successMessage) {
                    this.notificationService.notify(Notification.info(successMessage));
                }

                const errorMessage = params['errorMessage'];

                if (errorMessage) {
                    this.notificationService.notify(Notification.error(errorMessage));
                }
            });
    }

    public close(notification: Notification) {
        this.notifications.splice(this.notifications.indexOf(notification), 1);
    }
 }