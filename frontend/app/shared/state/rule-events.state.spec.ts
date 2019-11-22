/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
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
        expect(ruleEventsState.snapshot.isLoaded).toBeTruthy();
        expect(ruleEventsState.snapshot.isLoading).toBeFalsy();
        expect(ruleEventsState.snapshot.ruleEvents).toEqual(oldRuleEvents);
        expect(ruleEventsState.snapshot.ruleEventsPager.numberOfItems).toEqual(200);

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
    });

    it('should reset loading when loading failed', () => {
        rulesService.setup(x => x.getEvents(app, 10, 0, undefined))
            .returns(() => throwError('error'));

        ruleEventsState.load().pipe(onErrorResumeNext()).subscribe();

        expect(ruleEventsState.snapshot.isLoading).toBeFalsy();
    });

    it('should load page size from local store', () => {
        localStore.setup(x => x.getInt('rule-events.pageSize', 10))
            .returns(() => 25);

        const state = new RuleEventsState(appsState.object, dialogs.object, localStore.object, rulesService.object);

        expect(state.snapshot.ruleEventsPager.pageSize).toBe(25);
    });

    it('should show notification on load when reload is true', () => {
        ruleEventsState.load(true).subscribe();

        expect().nothing();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should load with new pagination when paging', () => {
        rulesService.setup(x => x.getEvents(app, 10, 10, undefined))
            .returns(() => of(new RuleEventsDto(200, [])));

        ruleEventsState.setPager(new Pager(200, 1, 10)).subscribe();

        expect().nothing();

        rulesService.verify(x => x.getEvents(app, 10, 10, undefined), Times.once());
        rulesService.verify(x => x.getEvents(app, 10, 0, undefined), Times.once());
    });

    it('should update page size in local store', () => {
        rulesService.setup(x => x.getEvents(app, 50, 0, undefined))
            .returns(() => of(new RuleEventsDto(200, [])));

        ruleEventsState.setPager(new Pager(200, 0, 50)).subscribe();

        localStore.verify(x => x.setInt('rule-events.pageSize', 50), Times.atLeastOnce());

        expect().nothing();
    });

    it('should load with rule id when filtered', () => {
        rulesService.setup(x => x.getEvents(app, 10, 0, '12'))
            .returns(() => of(new RuleEventsDto(200, [])));

        ruleEventsState.filterByRule('12').subscribe();

        expect().nothing();

        rulesService.verify(x => x.getEvents(app, 10, 0, '12'), Times.once());
    });

    it('should not load again when rule id has not changed', () => {
        rulesService.setup(x => x.getEvents(app, 10, 0, '12'))
            .returns(() => of(new RuleEventsDto(200, [])));

        ruleEventsState.filterByRule('12').subscribe();
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