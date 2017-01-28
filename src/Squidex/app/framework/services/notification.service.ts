/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

export const NotificationServiceFactory = () => {
    return new NotificationService();
};

export class Notification {
    constructor(
        public readonly message: string,
        public readonly messageType: string,
        public readonly displayTime: number = 10000
    ) {
    }

    public static error(message: string): Notification {
        return new Notification(message, 'error');
    }

    public static info(message: string): Notification {
        return new Notification(message, 'info');
    }
}

@Injectable()
export class NotificationService {
    private readonly notificationsStream$ = new Subject<Notification>();

    public get notifications(): Observable<Notification> {
        return this.notificationsStream$;
    }

    public notify(notification: Notification) {
        this.notificationsStream$.next(notification);
    }
}