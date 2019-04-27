/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';

import { DialogService } from '@app/shared';

import { EventConsumerDto, EventConsumersService } from './../services/event-consumers.service';
import { EventConsumersState } from './event-consumers.state';

describe('EventConsumersState', () => {
    const oldConsumers = [
        new EventConsumerDto('name1', false, false, 'error', '1'),
        new EventConsumerDto('name2', true,  true,  'error', '2')
    ];

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
                .returns(() => of(oldConsumers)).verifiable();

            eventConsumersState.load().subscribe();

            expect(eventConsumersState.snapshot.eventConsumers.values).toEqual(oldConsumers);
            expect(eventConsumersState.snapshot.isLoaded).toBeTruthy();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should show notification on load when reload is true', () => {
            eventConsumersService.setup(x => x.getEventConsumers())
                .returns(() => of(oldConsumers)).verifiable();

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
                .returns(() => of(oldConsumers)).verifiable();

            eventConsumersState.load().subscribe();
        });

        it('should unmark as stopped when started', () => {
            eventConsumersService.setup(x => x.putStart(oldConsumers[1].name))
                .returns(() => of({})).verifiable();

            eventConsumersState.start(oldConsumers[1]).subscribe();

            const es_1 = eventConsumersState.snapshot.eventConsumers.at(1);

            expect(es_1.isStopped).toBeFalsy();
        });

        it('should mark as stopped when stopped', () => {
            eventConsumersService.setup(x => x.putStop(oldConsumers[0].name))
                .returns(() => of({})).verifiable();

            eventConsumersState.stop(oldConsumers[0]).subscribe();

            const es_1 = eventConsumersState.snapshot.eventConsumers.at(0);

            expect(es_1.isStopped).toBeTruthy();
        });

        it('should mark as resetting when reset', () => {
            eventConsumersService.setup(x => x.putReset(oldConsumers[0].name))
                .returns(() => of({})).verifiable();

            eventConsumersState.reset(oldConsumers[0]).subscribe();

            const es_1 = eventConsumersState.snapshot.eventConsumers.at(0);

            expect(es_1.isResetting).toBeTruthy();
        });
    });
});