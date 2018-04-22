/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Observable } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import {
    AppsState,
    DateTime,
    DialogService
} from '@app/shared';

import { RuleEventsState } from './rule-events.state';

import {
    RuleEventDto,
    RuleEventsDto,
    RulesService
} from './../services/rules.service';

describe('RuleEventsState', () => {
    const app = 'my-app';

    const oldRuleEvents = [
        new RuleEventDto('id1', DateTime.now(), null, 'event1', 'description', 'dump1', 'result1', 'result1', 1),
        new RuleEventDto('id2', DateTime.now(), null, 'event2', 'description', 'dump2', 'result2', 'result2', 2)
    ];

    let appsState: IMock<AppsState>;
    let dialogs: IMock<DialogService>;
    let rulesService: IMock<RulesService>;
    let ruleEventsState: RuleEventsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        appsState = Mock.ofType<AppsState>();

        appsState.setup(x => x.appName)
            .returns(() => app);

        rulesService = Mock.ofType<RulesService>();

        rulesService.setup(x => x.getEvents(app, 10, 0))
            .returns(() => Observable.of(new RuleEventsDto(200, oldRuleEvents)));

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

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should load next page and prev page when paging', () => {
        rulesService.setup(x => x.getEvents(app, 10, 10))
            .returns(() => Observable.of(new RuleEventsDto(200, [])));

        ruleEventsState.goNext().subscribe();
        ruleEventsState.goPrev().subscribe();

        rulesService.verify(x => x.getEvents(app, 10, 10), Times.once());
        rulesService.verify(x => x.getEvents(app, 10, 0), Times.exactly(2));
    });

    it('should call service when enqueuing event', () => {
        rulesService.setup(x => x.enqueueEvent(app, oldRuleEvents[0].id))
            .returns(() => Observable.of({}));

        ruleEventsState.enqueue(oldRuleEvents[0]).subscribe();

        rulesService.verify(x => x.enqueueEvent(app, oldRuleEvents[0].id), Times.once());
    });
});