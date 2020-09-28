/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { IMock, It, Mock, Times } from 'typemoq';
import { DialogRequest, DialogService, DialogServiceFactory, Notification, Tooltip } from './dialog.service';
import { LocalStoreService } from './local-store.service';

describe('DialogService', () => {
    let localStore: IMock<LocalStoreService>;

    beforeEach(() => {
        localStore = Mock.ofType<LocalStoreService>();
    });

    it('should instantiate from factory', () => {
        const dialogService = DialogServiceFactory(localStore.object);

        expect(dialogService).toBeDefined();
    });

    it('should instantiate', () => {
        const dialogService = new DialogService(localStore.object);

        expect(dialogService).toBeDefined();
    });

    it('should create error notification', () => {
        const notification = Notification.error('MyError');

        expect(notification.displayTime).toBe(10000);
        expect(notification.message).toBe('MyError');
        expect(notification.messageType).toBe('danger');
    });

    it('should create info notification', () => {
        const notification = Notification.info('MyInfo');

        expect(notification.displayTime).toBe(10000);
        expect(notification.message).toBe('MyInfo');
        expect(notification.messageType).toBe('info');
    });

    [true, false].map(confirmed => {
        it(`should confirm dialog with ${confirmed}`, () => {
            const dialogService = new DialogService(localStore.object);

            let isCompleted = false;
            let isNext: boolean;

            dialogService.dialogs.subscribe(dialog => {
                dialog.complete(confirmed);
            });

            dialogService.confirm('MyTitle', 'MyText').subscribe(result => {
                isNext = result;
            }, undefined, () => {
                isCompleted = true;
            });

            expect(isCompleted).toBeTruthy();
            expect(isNext!).toEqual(confirmed);

            localStore.verify(x => x.setInt(It.isAnyString(), It.isAnyNumber()), Times.never());
        });
    });

    [true, false].map(confirmed => {
        it(`should confirm dialog with '${confirmed}' but not remember`, () => {
            const dialogService = new DialogService(localStore.object);

            let isCompleted = false;
            let isNext: boolean;

            dialogService.dialogs.subscribe(dialog => {
                dialog.complete(confirmed);
            });

            dialogService.confirm('MyTitle', 'MyText').subscribe(result => {
                isNext = result;
            }, undefined, () => {
                isCompleted = true;
            });

            expect(isCompleted).toBeTruthy();
            expect(isNext!).toEqual(confirmed);

            localStore.verify(x => x.setInt(It.isAnyString(), It.isAnyNumber()), Times.never());
        });
    });

    [
        { confirmed: true, saved: 1 },
        { confirmed: false, saved: 2 }
    ].map(({ confirmed, saved }) => {
        it(`should confirm dialog with '${confirmed}' and remember when remembered`, () => {
            const dialogService = new DialogService(localStore.object);

            let isCompleted = false;
            let isNext: boolean;

            dialogService.dialogs.subscribe(dialog => {
                dialog.remember = true;
                dialog.complete(confirmed);
            });

            dialogService.confirm('MyTitle', 'MyText', 'MyKey').subscribe(result => {
                isNext = result;
            }, undefined, () => {
                isCompleted = true;
            });

            expect(isCompleted).toBeTruthy();
            expect(isNext!).toEqual(confirmed);

            localStore.verify(x => x.setInt(`dialogs.confirm.MyKey`, saved), Times.once());
        });
    });

    [
        { confirmed: true, saved: 1 },
        { confirmed: false, saved: 2 }
    ].map(({ confirmed, saved }) => {
        it(`should confirm dialog with '${confirmed}' from local store when saved`, () => {
            const dialogService = new DialogService(localStore.object);

            localStore.setup(x => x.getInt(`dialogs.confirm.MyKey`))
                .returns(() => saved);

            let requestCount = 0;

            let isCompleted = false;
            let isNext: boolean;

            dialogService.dialogs.subscribe(_ => {
                requestCount++;
            });

            dialogService.confirm('MyTitle', 'MyText', 'MyKey').subscribe(result => {
                isNext = result;
            }, undefined, () => {
                isCompleted = true;
            });

            expect(isCompleted).toBeTruthy();
            expect(isNext!).toEqual(confirmed);
            expect(requestCount).toEqual(0);
        });
    });

    it('should publish tooltip', () => {
        const dialogService = new DialogService(localStore.object);

        const tooltip = new Tooltip('target', 'text', 'left');

        let publishedTooltip: Tooltip;

        dialogService.tooltips.subscribe(result => {
            publishedTooltip = result;
        });

        dialogService.tooltip(tooltip);

        expect(publishedTooltip!).toBe(tooltip);
    });

    it('should publish notification', () => {
        const dialogService = new DialogService(localStore.object);

        const notification = Notification.error('Message');

        let publishedNotification: Notification;

        dialogService.notifications.subscribe(result => {
            publishedNotification = result;
        });

        dialogService.notify(notification);

        expect(publishedNotification!).toBe(notification);
    });

    it('should publish dialog request', () => {
        const dialogService = new DialogService(localStore.object);

        let pushedDialog: DialogRequest;

        dialogService.dialogs.subscribe(result => {
            pushedDialog = result;
        });

        dialogService.confirm('MyTitle', 'MyText');

        expect(pushedDialog!).toBeDefined();
    });
});
