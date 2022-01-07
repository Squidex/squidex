/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DialogService, RulesService } from '@app/shared/internal';
import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';
import { SimulatedRuleEventsDto } from '../services/rules.service';
import { createSimulatedRuleEvent } from './../services/rules.service.spec';
import { RuleSimulatorState } from './rule-simulator.state';
import { TestValues } from './_test-helpers';

describe('RuleSimulatorState', () => {
    const {
        app,
        appsState,
    } = TestValues;

    const oldSimulatedRuleEvents = [
        createSimulatedRuleEvent(1),
        createSimulatedRuleEvent(2),
    ];

    let dialogs: IMock<DialogService>;
    let rulesService: IMock<RulesService>;
    let ruleSimulatorState: RuleSimulatorState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        rulesService = Mock.ofType<RulesService>();

        ruleSimulatorState = new RuleSimulatorState(appsState.object, dialogs.object, rulesService.object);
    });

    it('should load simulated rule events', () => {
        rulesService.setup(x => x.getSimulatedEvents(app, '12'))
            .returns(() => of(new SimulatedRuleEventsDto(200, oldSimulatedRuleEvents)));

        ruleSimulatorState.selectRule('12');
        ruleSimulatorState.load().subscribe();

        expect(ruleSimulatorState.snapshot.simulatedRuleEvents).toEqual(oldSimulatedRuleEvents);
        expect(ruleSimulatorState.snapshot.isLoaded).toBeTruthy();
        expect(ruleSimulatorState.snapshot.isLoading).toBeFalsy();
        expect(ruleSimulatorState.snapshot.total).toEqual(200);

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
    });

    it('should reset loading state if loading failed', () => {
        rulesService.setup(x => x.getSimulatedEvents(app, '12'))
            .returns(() => throwError(() => 'Service Error'));

        ruleSimulatorState.selectRule('12');
        ruleSimulatorState.load().pipe(onErrorResumeNext()).subscribe();

        expect(ruleSimulatorState.snapshot.isLoading).toBeFalsy();
    });

    it('should not load simulated rule events if no rule selected', () => {
        ruleSimulatorState.load().subscribe();

        expect().nothing();

        rulesService.verify(x => x.getSimulatedEvents(app, It.isAnyString()), Times.never());
    });
});
