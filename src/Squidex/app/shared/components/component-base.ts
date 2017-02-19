/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Observable } from 'rxjs';

import {
    ErrorDto,
    Notification,
    NotificationService,
    UsersProviderService
} from 'shared';

export abstract class ComponentBase {
    constructor(
        private readonly notifications: NotificationService,
        private readonly users: UsersProviderService
    ) {
    }

    public userEmail(userId: string, isRef: boolean = false): Observable<string> {
        if (isRef) {
            const parts = userId.split(':');

            if (parts[0] === 'subject') {
                return this.users.getUser(parts[1]).map(u => u.email);
            } else {
                return null;
            }
        } else {
            return this.users.getUser(userId).map(u => u.email);
        }
    }

    public userPicture(userId: string, isRef: boolean = false): Observable<string> {
        if (isRef) {
            const parts = userId.split(':');

            if (parts[0] === 'subject') {
                return this.users.getUser(parts[1]).map(u => u.pictureUrl);
            } else {
                return null;
            }
        } else {
            return this.users.getUser(userId).map(u => u.pictureUrl);
        }
    }

    public userName(userId: string, isRef: boolean = false, placeholder = 'Me'): Observable<string> {
        if (isRef) {
            const parts = userId.split(':');

            if (parts[0] === 'subject') {
                return this.users.getUser(parts[1], placeholder).map(u => u.displayName);
            } else {
                return Observable.of(parts[1]);
            }
        } else {
            return this.users.getUser(userId, placeholder).map(u => u.displayName);
        }
    }

    protected notifyError(error: string | ErrorDto) {
        if (error instanceof ErrorDto) {
            this.notifications.notify(Notification.error(error.displayMessage));
        } else {
            this.notifications.notify(Notification.error(error));
        }
    }

    protected notifyInfo(error: string) {
        this.notifications.notify(Notification.error(error));
    }
}