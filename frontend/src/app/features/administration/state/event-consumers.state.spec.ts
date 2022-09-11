/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';
import { EventConsumersService } from '@app/features/administration/internal';
import { DialogService } from '@app/framework';
import { createEventConsumer } from './../services/event-consumers.service.spec';
import { EventConsumersState } from './event-consumers.state';

describe('EventConsumersState', () => {
    const eventConsumer1 = createEventConsumer(1);
    const eventConsumer2 = createEventConsumer(2);

    let dialogs: IMock<DialogService>;
    let eventConsumersService: IMock<EventConsumersService>;
    let eventConsumersState: EventConsumersState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        eventConsumersService = Mock.ofType<EventConsumersService>();
        eventConsumersState = new EventConsumersState(dialogs.object, eventConsumersService.object);
    });

    afterEach(() => {
        eventConsumersService.verifyAll();
    });

    describe('Loading', () => {
        it('should load event consumers', () => {
            eventConsumersService.setup(x => x.getEventConsumers())
                .returns(() => of({ items: [eventConsumer1, eventConsumer2] })).verifiable();

            eventConsumersState.load().subscribe();

            expect(eventConsumersState.snapshot.eventConsumers).toEqual([eventConsumer1, eventConsumer2]);
            expect(eventConsumersState.snapshot.isLoaded).toBeTruthy();
            expect(eventConsumersState.snapshot.isLoading).toBeFalsy();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading state if loading failed', () => {
            eventConsumersService.setup(x => x.getEventConsumers())
                .returns(() => throwError(() => 'Service Error'));

            eventConsumersState.load().pipe(onErrorResumeNext()).subscribe();

            expect(eventConsumersState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            eventConsumersService.setup(x => x.getEventConsumers())
            .returns(() => of({ items: [eventConsumer1, eventConsumer2] })).verifiable();

            eventConsumersState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should show notification on load error if silent is false', () => {
            eventConsumersService.setup(x => x.getEventConsumers())
                .returns(() => throwError(() => 'Service Error')).verifiable();

            eventConsumersState.load(true, false).pipe(onErrorResumeNext()).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyError(It.isAny()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            eventConsumersService.setup(x => x.getEventConsumers())
            .returns(() => of({ items: [eventConsumer1, eventConsumer2] })).verifiable();

            eventConsumersState.load().subscribe();
        });

        it('should update event consumer if started', () => {
            const updated = createEventConsumer(2, '_new');

            eventConsumersService.setup(x => x.putStart(eventConsumer2))
                .returns(() => of(updated)).verifiable();

            eventConsumersState.start(eventConsumer2).subscribe();

            expect(eventConsumersState.snapshot.eventConsumers).toEqual([eventConsumer1, updated]);
        });

        it('should update event consumer if stopped', () => {
            const updated = createEventConsumer(2, '_new');

            eventConsumersService.setup(x => x.putStop(eventConsumer2))
                .returns(() => of(updated)).verifiable();

            eventConsumersState.stop(eventConsumer2).subscribe();

            expect(eventConsumersState.snapshot.eventConsumers).toEqual([eventConsumer1, updated]);
        });

        it('should update event consumer if reset', () => {
            const updated = createEventConsumer(2, '_new');

            eventConsumersService.setup(x => x.putReset(eventConsumer2))
                .returns(() => of(updated)).verifiable();

            eventConsumersState.reset(eventConsumer2).subscribe();

            expect(eventConsumersState.snapshot.eventConsumers).toEqual([eventConsumer1, updated]);
        });
    });
});
