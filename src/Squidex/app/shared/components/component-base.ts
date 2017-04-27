/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import {
    ErrorDto,
    Notification,
    NotificationService
} from './../declarations-base';

export abstract class ComponentBase {
    constructor(
        private readonly notifications: NotificationService
    ) {
    }

    protected notifyError(error: string | ErrorDto) {
        if (error instanceof ErrorDto) {
            this.notifications.notify(Notification.error(error.displayMessage));
        } else {
            this.notifications.notify(Notification.error(error));
        }
    }

    protected notifyInfo(error: string) {
        this.notifications.notify(Notification.info(error));
    }
}