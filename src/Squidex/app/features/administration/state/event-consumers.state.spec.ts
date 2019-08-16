/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';

import { DialogService } from '@app/framework';

import { EventConsumersDto, EventConsumersService } from '@app/features/administration/internal';
import { EventConsumersState } from './event-consumers.state';

import { createEventConsumer } from './../services/event-consumers.service.spec';

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
                .returns(() => of(new EventConsumersDto([eventConsumer1, eventConsumer2]))).verifiable();

            eventConsumersState.load().subscribe();

            expect(eventConsumersState.snapshot.eventConsumers.values).toEqual([eventConsumer1, eventConsumer2]);
            expect(eventConsumersState.snapshot.isLoaded).toBeTruthy();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should show notification on load when reload is true', () => {
            eventConsumersService.setup(x => x.getEventConsumers())
                .returns(() => of(new EventConsumersDto([eventConsumer1, eventConsumer2]))).verifiable();

            eventConsumersState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should show notification on load error when silent is false', () => {
            eventConsumersService.setup(x => x.getEventConsumers())
                .returns(() => throwError({})).verifiable();

            eventConsumersState.load(true, false).pipe(onErrorResumeNext()).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyError(It.isAny()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            eventConsumersService.setup(x => x.getEventConsumers())
                .returns(() => of(new EventConsumersDto([eventConsumer1, eventConsumer2]))).verifiable();

            eventConsumersState.load().subscribe();
        });

        it('should update event consumer when started', () => {
            const updated = createEventConsumer(2, '_new');

            eventConsumersService.setup(x => x.putStart(eventConsumer2))
                .returns(() => of(updated)).verifiable();

            eventConsumersState.start(eventConsumer2).subscribe();

            const newConsumer2 = eventConsumersState.snapshot.eventConsumers.at(1);

            expect(newConsumer2).toEqual(updated);
        });

        it('should update event consumer when stopped', () => {
            const updated = createEventConsumer(2, '_new');

            eventConsumersService.setup(x => x.putStop(eventConsumer2))
                .returns(() => of(updated)).verifiable();

            eventConsumersState.stop(eventConsumer2).subscribe();

            const newConsumer2 = eventConsumersState.snapshot.eventConsumers.at(1);

            expect(newConsumer2).toEqual(updated);
        });

        it('should update event consumer when reset', () => {
            const updated = createEventConsumer(2, '_new');

            eventConsumersService.setup(x => x.putReset(eventConsumer2))
                .returns(() => of(updated)).verifiable();

            eventConsumersState.reset(eventConsumer2).subscribe();

            const newConsumer2 = eventConsumersState.snapshot.eventConsumers.at(1);

            expect(newConsumer2).toEqual(updated);
        });
    });
});