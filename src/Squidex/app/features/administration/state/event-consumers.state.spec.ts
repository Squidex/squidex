/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Observable } from 'rxjs';
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

        eventConsumersService.setup(x => x.getEventConsumers())
            .returns(() => Observable.of(oldConsumers));

        eventConsumersState = new EventConsumersState(dialogs.object, eventConsumersService.object);
        eventConsumersState.load().subscribe();
    });

    it('should load event consumers', () => {
        expect(eventConsumersState.snapshot.eventConsumers.values).toEqual(oldConsumers);
        expect(eventConsumersState.snapshot.isLoaded).toBeTruthy();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
    });

    it('should show notification on load when reload is true', () => {
        eventConsumersState.load(true, true).subscribe();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should show notification on load error when silent is true', () => {
        eventConsumersService.setup(x => x.getEventConsumers())
            .returns(() => Observable.throw({}));

        eventConsumersState.load(true, true).onErrorResumeNext().subscribe();

        dialogs.verify(x => x.notifyError(It.isAny()), Times.once());
    });

    it('should not show notification on load error when flag is false', () => {
        eventConsumersService.setup(x => x.getEventConsumers())
            .returns(() => Observable.throw({}));

        eventConsumersState.load().onErrorResumeNext().subscribe();

        dialogs.verify(x => x.notifyError(It.isAny()), Times.never());
    });

    it('should unmark as stopped when started', () => {
        eventConsumersService.setup(x => x.putStart(oldConsumers[1].name))
            .returns(() => Observable.of({}));

        eventConsumersState.start(oldConsumers[1]).subscribe();

        const es_1 = eventConsumersState.snapshot.eventConsumers.at(1);

        expect(es_1.isStopped).toBeFalsy();
    });

    it('should mark as stopped when stopped', () => {
        eventConsumersService.setup(x => x.putStop(oldConsumers[0].name))
            .returns(() => Observable.of({}));

        eventConsumersState.stop(oldConsumers[0]).subscribe();

        const es_1 = eventConsumersState.snapshot.eventConsumers.at(0);

        expect(es_1.isStopped).toBeTruthy();
    });

    it('should mark as resetting when reset', () => {
        eventConsumersService.setup(x => x.putReset(oldConsumers[0].name))
            .returns(() => Observable.of({}));

        eventConsumersState.reset(oldConsumers[0]).subscribe();

        const es_1 = eventConsumersState.snapshot.eventConsumers.at(0);

        expect(es_1.isResetting).toBeTruthy();
    });
});