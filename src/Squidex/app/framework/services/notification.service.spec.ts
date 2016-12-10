/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import {
    Notification,
    NotificationService,
    NotificationServiceFactory
} from './../';

describe('NotificationService', () => {
    it('should instantiate from factory', () => {
        const notificationService = NotificationServiceFactory();

        expect(notificationService).toBeDefined();
    });

    it('should instantiate', () => {
        const notificationService = new NotificationService();

        expect(notificationService).toBeDefined();
    });

    it('should publish notification', () => {
        const notificationService = new NotificationService();
        const notification = Notification.error('Message');

        let publishedNotification: Notification;

        notificationService.notifications.subscribe(result => {
            publishedNotification = result;
        });

        notificationService.notify(notification);

        expect(publishedNotification).toBe(notification);
    });
});
