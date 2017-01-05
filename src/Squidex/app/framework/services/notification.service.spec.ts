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

    it('should create error', () => {
        const notification = Notification.error('MyError');

        expect(notification.displayTime).toBe(10000);
        expect(notification.message).toBe('MyError');
        expect(notification.messageType).toBe('error');
    });

    it('should create info', () => {
        const notification = Notification.info('MyInfo');

        expect(notification.displayTime).toBe(10000);
        expect(notification.message).toBe('MyInfo');
        expect(notification.messageType).toBe('info');
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
