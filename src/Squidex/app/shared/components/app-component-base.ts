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
        return this.appsStore.selectedApp.map(a => a!.name).take(1);
    }

    public userEmail(userId: string, isRef: boolean = false): Observable<string> {
        if (isRef) {
            const parts = userId.split(':');

            if (parts[0] === 'subject') {
                return this.usersProvider.getUser(parts[1]).map(u => u.email);
            } else {
                return null;
            }
        } else {
            return this.usersProvider.getUser(userId).map(u => u.email);
        }
    }

    public userPicture(userId: string, isRef: boolean = false): Observable<string> {
        if (isRef) {
            const parts = userId.split(':');

            if (parts[0] === 'subject') {
                return this.usersProvider.getUser(parts[1]).map(u => u.pictureUrl);
            } else {
                return null;
            }
        } else {
            return this.usersProvider.getUser(userId).map(u => u.pictureUrl);
        }
    }

    public userName(userId: string, isRef: boolean = false, placeholder = 'Me'): Observable<string> {
        if (isRef) {
            const parts = userId.split(':');

            if (parts[0] === 'subject') {
                return this.usersProvider.getUser(parts[1], placeholder).map(u => u.displayName);
            } else {
                return Observable.of(parts[1]);
            }
        } else {
            return this.usersProvider.getUser(userId, placeholder).map(u => u.displayName);
        }
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

