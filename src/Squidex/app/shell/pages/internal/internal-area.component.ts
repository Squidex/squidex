/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import {
    Notification,
    NotificationService
} from 'shared';

@Ng2.Component({
    selector: 'sqx-internal-area',
    styles,
    template
})
export class InternalAreaComponent implements Ng2.OnInit, Ng2.OnDestroy {
    private notificationsSubscription: any;

    public notifications: Notification[] = [];

    constructor(
        private readonly notificationService: NotificationService
    ) {
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

    public ngOnDestroy() {
        this.notificationsSubscription.unsubscribe();
    }

    public close(notification: Notification) {
        this.notifications.splice(this.notifications.indexOf(notification), 1);
    }
 }