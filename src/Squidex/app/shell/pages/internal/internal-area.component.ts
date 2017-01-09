/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
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

    public notifications: Notification[] = [];

    constructor(
        private readonly notificationService: NotificationService
    ) {
    }

    public ngOnDestroy() {
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
    }

    public close(notification: Notification) {
        this.notifications.splice(this.notifications.indexOf(notification), 1);
    }
 }