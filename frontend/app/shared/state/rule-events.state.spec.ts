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
    LocalStoreService,
    Pager,
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
    let localStore: IMock<LocalStoreService>;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        localStore = Mock.ofType<LocalStoreService>();

        rulesService = Mock.ofType<RulesService>();
        rulesService.setup(x => x.getEvents(app, 10, 0, undefined))
            .returns(() => of(new RuleEventsDto(200, oldRuleEvents)));

        ruleEventsState = new RuleEventsState(appsState.object, dialogs.object, localStore.object, rulesService.object);
        ruleEventsState.load().subscribe();
    });

    it('should load ruleEvents', () => {
        expect(ruleEventsState.snapshot.ruleEvents).toEqual(oldRuleEvents);
        expect(ruleEventsState.snapshot.ruleEventsPager.numberOfItems).toEqual(200);
        expect(ruleEventsState.snapshot.isLoaded).toBeTruthy();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
    });

    it('should show notification on load when reload is true', () => {
        ruleEventsState.load(true).subscribe();

        expect().nothing();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should load with new pagination when paging', () => {
        rulesService.setup(x => x.getEvents(app, 10, 10, undefined))
            .returns(() => of(new RuleEventsDto(200, [])));

        ruleEventsState.setPager(new Pager(20, 1, 10));

        expect().nothing();

        rulesService.verify(x => x.getEvents(app, 10, 10, undefined), Times.once());
        rulesService.verify(x => x.getEvents(app, 10, 0, undefined), Times.once());
    });

    it('should load with rule id when filtered', () => {
        rulesService.setup(x => x.getEvents(app, 10, 0, '12'))
            .returns(() => of(new RuleEventsDto(200, [])));

        ruleEventsState.filterByRule('12').subscribe();

        expect().nothing();

        rulesService.verify(x => x.getEvents(app, 10, 0, '12'), Times.once());
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