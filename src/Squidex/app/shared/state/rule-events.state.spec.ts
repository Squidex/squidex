/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    DialogService,
    RuleEventsDto,
    RuleEventsState,
    RulesService
} from '@app/shared/internal';

import { createRuleEvent } from '../services/rules.service.spec';

import { TestValues } from './_test-helpers';

describe('RuleEventsState', () => {
    const {
        app,
        appsState
    } = TestValues;

    const oldRuleEvents = [
         createRuleEvent(1),
         createRuleEvent(2)
    ];

    let dialogs: IMock<DialogService>;
    let rulesService: IMock<RulesService>;
    let ruleEventsState: RuleEventsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        rulesService = Mock.ofType<RulesService>();

        rulesService.setup(x => x.getEvents(app, 10, 0))
            .returns(() => of(new RuleEventsDto(200, oldRuleEvents)));

        ruleEventsState = new RuleEventsState(appsState.object, dialogs.object, rulesService.object);
        ruleEventsState.load().subscribe();
    });

    it('should load ruleEvents', () => {
        expect(ruleEventsState.snapshot.ruleEvents.values).toEqual(oldRuleEvents);
        expect(ruleEventsState.snapshot.ruleEventsPager.numberOfItems).toEqual(200);
        expect(ruleEventsState.snapshot.isLoaded).toBeTruthy();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
    });

    it('should show notification on load when reload is true', () => {
        ruleEventsState.load(true).subscribe();

        expect().nothing();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should load next page and prev page when paging', () => {
        rulesService.setup(x => x.getEvents(app, 10, 10))
            .returns(() => of(new RuleEventsDto(200, [])));

        ruleEventsState.goNext().subscribe();
        ruleEventsState.goPrev().subscribe();

        expect().nothing();

        rulesService.verify(x => x.getEvents(app, 10, 10), Times.once());
        rulesService.verify(x => x.getEvents(app, 10, 0), Times.exactly(2));
    });

    it('should call service when enqueuing event', () => {
        rulesService.setup(x => x.enqueueEvent(app, oldRuleEvents[0]))
            .returns(() => of({}));

        ruleEventsState.enqueue(oldRuleEvents[0]).subscribe();

        expect().nothing();

        rulesService.verify(x => x.enqueueEvent(app, oldRuleEvents[0]), Times.once());
    });

    it('should call service when cancelling event', () => {
        rulesService.setup(x => x.cancelEvent(app, oldRuleEvents[0]))
            .returns(() => of({}));

        ruleEventsState.cancel(oldRuleEvents[0]).subscribe();

        expect().nothing();

        rulesService.verify(x => x.cancelEvent(app, oldRuleEvents[0]), Times.once());
    });
});