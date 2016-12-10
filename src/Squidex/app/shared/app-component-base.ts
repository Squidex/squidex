/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Observable } from 'rxjs';

import {
    AppsStoreService,
    ErrorDto,
    Notification,
    NotificationService,
    UsersProviderService
} from 'shared';

export abstract class AppComponentBase {
    constructor(
        private readonly appsStore: AppsStoreService,
        private readonly notifications: NotificationService,
        private readonly usersProvider: UsersProviderService
    ) {
    }

    public appName(): Observable<string> {
        return this.appsStore.selectedApp.map(a => a.name);
    }

    public userEmail(userId: string): Observable<string> {
        return this.usersProvider.getUser(userId).map(u => u.email);
    }

    public userName(userId: string): Observable<string> {
        return this.usersProvider.getUser(userId).map(u => u.displayName);
    }

    public userPicture(userId: string): Observable<string> {
        return this.usersProvider.getUser(userId).map(u => u.pictureUrl);
    }

    public notifyError(error: string | ErrorDto) {
        if (error instanceof ErrorDto) {
            this.notifications.notify(Notification.error(error.displayMessage));
        } else {
            this.notifications.notify(Notification.error(error));
        }
    }

    public notifyInfo(error: string) {
        this.notifications.notify(Notification.error(error));
    }
}

