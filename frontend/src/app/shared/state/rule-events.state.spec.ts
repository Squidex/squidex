/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, onErrorResumeNextWith, throwError } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { DialogService, RuleEventsDto, RuleEventsState, RulesService } from '@app/shared/internal';
import { createRuleEvent } from '../services/rules.service.spec';
import { TestValues } from './_test-helpers';

describe('RuleEventsState', () => {
    const {
        app,
        appsState,
    } = TestValues;

    const oldRuleEvents = new RuleEventsDto({
        total: 200,
        items: [
            createRuleEvent(1),
            createRuleEvent(2),
        ],
        _links: {},
    });

    let dialogs: IMock<DialogService>;
    let rulesService: IMock<RulesService>;
    let ruleEventsState: RuleEventsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        rulesService = Mock.ofType<RulesService>();
        rulesService.setup(x => x.getEvents(app, 30, 0, undefined))
            .returns(() => of(oldRuleEvents));

        ruleEventsState = new RuleEventsState(appsState.object, dialogs.object, rulesService.object);
        ruleEventsState.load().subscribe();
    });

    it('should load rule events', () => {
        expect(ruleEventsState.snapshot.ruleEvents).toEqual(oldRuleEvents.items);
        expect(ruleEventsState.snapshot.isLoaded).toBeTruthy();
        expect(ruleEventsState.snapshot.isLoading).toBeFalsy();
        expect(ruleEventsState.snapshot.total).toEqual(200);

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
    });

    it('should reset loading state if loading failed', () => {
        rulesService.setup(x => x.getEvents(app, 30, 0, undefined))
            .returns(() => throwError(() => 'Service Error'));

        ruleEventsState.load().pipe(onErrorResumeNextWith()).subscribe();

        expect(ruleEventsState.snapshot.isLoading).toBeFalsy();
    });

    it('should show notification on load if reload is true', () => {
        ruleEventsState.load(true).subscribe();

        expect().nothing();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should load with new pagination if paging', () => {
        rulesService.setup(x => x.getEvents(app, 30, 30, undefined))
            .returns(() => of(new RuleEventsDto({ items: [], total: 0, _links: {} })));

        ruleEventsState.page({ page: 1, pageSize: 30 }).subscribe();

        expect().nothing();

        rulesService.verify(x => x.getEvents(app, 30, 30, undefined), Times.once());
        rulesService.verify(x => x.getEvents(app, 30, 0, undefined), Times.once());
    });

    it('should load with rule id if filtered', () => {
        rulesService.setup(x => x.getEvents(app, 30, 0, '12'))
        .returns(() => of(new RuleEventsDto({ items: [], total: 200, _links: {} })));

        ruleEventsState.filterByRule('12').subscribe();

        expect().nothing();

        rulesService.verify(x => x.getEvents(app, 30, 0, '12'), Times.once());
    });

    it('should not load again if rule id has not changed', () => {
        rulesService.setup(x => x.getEvents(app, 30, 0, '12'))
        .returns(() => of(new RuleEventsDto({ items: [], total: 200, _links: {} })));

        ruleEventsState.filterByRule('12').subscribe();
        ruleEventsState.filterByRule('12').subscribe();

        expect().nothing();

        rulesService.verify(x => x.getEvents(app, 30, 0, '12'), Times.once());
    });

    it('should call service if enqueuing event', () => {
        rulesService.setup(x => x.enqueueEvent(app, oldRuleEvents.items[0]))
            .returns(() => of({}));

        ruleEventsState.enqueue(oldRuleEvents.items[0]).subscribe();

        expect().nothing();

        rulesService.verify(x => x.enqueueEvent(app, oldRuleEvents.items[0]), Times.once());
    });

    it('should call service if cancelling event', () => {
        rulesService.setup(x => x.cancelEvents(app, oldRuleEvents.items[0]))
            .returns(() => of({}));

        ruleEventsState.cancel(oldRuleEvents.items[0]).subscribe();

        expect().nothing();

        rulesService.verify(x => x.cancelEvents(app, oldRuleEvents.items[0]), Times.once());
    });

    it('should call service if cancelling all events', () => {
        rulesService.setup(x => x.cancelEvents(app, It.isAny()))
            .returns(() => of({}));

        ruleEventsState.cancelAll().subscribe();

        expect().nothing();

        rulesService.verify(x => x.cancelEvents(app, It.isAny()), Times.once());
    });
});
