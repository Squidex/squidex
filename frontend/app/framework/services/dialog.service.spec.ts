/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { IMock, It, Mock, Times } from 'typemoq';
import { DialogRequest, DialogService, Notification, Tooltip } from './dialog.service';
import { LocalStoreService } from './local-store.service';

describe('DialogService', () => {
    let localStore: IMock<LocalStoreService>;

    beforeEach(() => {
        localStore = Mock.ofType<LocalStoreService>();
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
        expect(notification.messageType).toBe('primary');
    });

    [true, false].forEach(confirmed => {
        it(`should confirm dialog with ${confirmed}`, () => {
            const dialogService = new DialogService(localStore.object);

            let isCompleted = false;
            let isNext: boolean;

            dialogService.dialogs.subscribe(dialog => {
                dialog.complete(confirmed);
            });

            dialogService.confirm('MyTitle', 'MyText').subscribe({
                next: result => {
                    isNext = result;
                },
                complete: () => {
                    isCompleted = true;
                },
            });

            expect(isCompleted).toBeTruthy();
            expect(isNext!).toEqual(confirmed);

            localStore.verify(x => x.setInt(It.isAnyString(), It.isAnyNumber()), Times.never());
        });
    });

    [true, false].forEach(confirmed => {
        it(`should confirm dialog with '${confirmed}' but not remember`, () => {
            const dialogService = new DialogService(localStore.object);

            let isCompleted = false;
            let isNext: boolean;

            dialogService.dialogs.subscribe(dialog => {
                dialog.complete(confirmed);
            });

            dialogService.confirm('MyTitle', 'MyText').subscribe({
                next: result => {
                    isNext = result;
                },
                complete: () => {
                    isCompleted = true;
                },
            });

            expect(isCompleted).toBeTruthy();
            expect(isNext!).toEqual(confirmed);

            localStore.verify(x => x.setInt(It.isAnyString(), It.isAnyNumber()), Times.never());
        });
    });

    [
        { confirmed: true, saved: 1 },
        { confirmed: false, saved: 0 },
    ].forEach(({ confirmed, saved }) => {
        it(`should confirm dialog with '${confirmed}' and remember if remembered and confirmed`, () => {
            const dialogService = new DialogService(localStore.object);

            let isCompleted = false;
            let isNext: boolean;

            dialogService.dialogs.subscribe(dialog => {
                dialog.remember = true;
                dialog.complete(confirmed);
            });

            dialogService.confirm('MyTitle', 'MyText', 'MyKey').subscribe({
                next: result => {
                    isNext = result;
                },
                complete: () => {
                    isCompleted = true;
                },
            });

            expect(isCompleted).toBeTruthy();
            expect(isNext!).toEqual(confirmed);

            localStore.verify(x => x.setBoolean('dialogs.confirm.MyKey', It.isAny()), Times.exactly(saved));
        });
    });

    [
        { confirmed: true, saved: true, render: 0 },
        { confirmed: false, saved: false, render: 1 },
    ].forEach(({ confirmed, saved, render }) => {
        it(`should confirm dialog with '${confirmed}' from local store if saved`, () => {
            const dialogService = new DialogService(localStore.object);

            localStore.setup(x => x.getBoolean('dialogs.confirm.MyKey'))
                .returns(() => saved);

            let requestCount = 0;

            let isCompleted = false;
            let isNext: boolean | undefined;

            dialogService.dialogs.subscribe(_ => {
                requestCount++;
            });

            dialogService.confirm('MyTitle', 'MyText', 'MyKey').subscribe({
                next: result => {
                    isNext = result;
                },
                complete: () => {
                    isCompleted = true;
                },
            });

            expect(isCompleted).toEqual(confirmed);
            expect(isNext).toEqual(confirmed ? true : undefined);
            expect(requestCount).toEqual(render);
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
