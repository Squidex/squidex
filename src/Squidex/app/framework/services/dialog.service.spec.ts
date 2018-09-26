/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import {
    DialogRequest,
    DialogService,
    DialogServiceFactory,
    Notification
} from './dialog.service';

describe('DialogService', () => {
    it('should instantiate from factory', () => {
        const dialogService = DialogServiceFactory();

        expect(dialogService).toBeDefined();
    });

    it('should instantiate', () => {
        const dialogService = new DialogService();

        expect(dialogService).toBeDefined();
    });

    it('should create error notification', () => {
        const notification = Notification.error('MyError');

        expect(notification.displayTime).toBe(5000);
        expect(notification.message).toBe('MyError');
        expect(notification.messageType).toBe('danger');
    });

    it('should create info notification', () => {
        const notification = Notification.info('MyInfo');

        expect(notification.displayTime).toBe(5000);
        expect(notification.message).toBe('MyInfo');
        expect(notification.messageType).toBe('info');
    });

    it('should create dialog request', () => {
        const dialog = new DialogRequest('MyTitle', 'MyText');

        expect(dialog.title).toBe('MyTitle');
        expect(dialog.text).toBe('MyText');
    });

    it('should confirm dialog', () => {
        const dialog = new DialogRequest('MyTitle', 'MyText');

        let isCompleted = false;
        let isNext = false;

        dialog.closed.subscribe(result => {
            isNext = result;
        }, undefined, () => {
            isCompleted = true;
        });

        dialog.complete(true);

        expect(isCompleted).toBeTruthy();
        expect(isNext).toBeTruthy();
    });

    it('should publish notification', () => {
        const dialogService = new DialogService();
        const notification = Notification.error('Message');

        let publishedNotification: Notification;

        dialogService.notifications.subscribe(result => {
            publishedNotification = result;
        });

        dialogService.notify(notification);

        expect(publishedNotification!).toBe(notification);
    });

    it('should publish dialog request', () => {
        const dialogService = new DialogService();

        let pushedDialog: DialogRequest;

        dialogService.dialogs.subscribe(result => {
            pushedDialog = result;
        });

        dialogService.confirm('MyTitle', 'MyText');

        expect(pushedDialog!).toEqual(new DialogRequest('MyTitle', 'MyText'));
    });
});
