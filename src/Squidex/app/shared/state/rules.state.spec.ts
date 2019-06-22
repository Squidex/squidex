/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';

import { RulesState } from './rules.state';

import {
    DialogService,
    RulesDto,
    RulesService,
    versioned
} from '@app/shared/internal';

import { createRule } from '../services/rules.service.spec';

import { TestValues } from './_test-helpers';

describe('RulesState', () => {
    const {
        app,
        appsState,
        newVersion,
        version
    } = TestValues;

    const rule1 = createRule(1);
    const rule2 = createRule(2);

    const newRule = createRule(3);

    let dialogs: IMock<DialogService>;
    let rulesService: IMock<RulesService>;
    let rulesState: RulesState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        rulesService = Mock.ofType<RulesService>();
        rulesState = new RulesState(appsState.object, dialogs.object, rulesService.object);
    });

    afterEach(() => {
        rulesService.verifyAll();
    });

    describe('Loading', () => {
        it('should load rules', () => {
            rulesService.setup(x => x.getRules(app))
                .returns(() => of(new RulesDto([rule1, rule2]))).verifiable();

            rulesState.load().subscribe();

            expect(rulesState.snapshot.rules.values).toEqual([rule1, rule2]);
            expect(rulesState.snapshot.isLoaded).toBeTruthy();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should show notification on load when reload is true', () => {
            rulesService.setup(x => x.getRules(app))
                .returns(() => of(new RulesDto([rule1, rule2]))).verifiable();

            rulesState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

    });

    describe('Updates', () => {
        beforeEach(() => {
            rulesService.setup(x => x.getRules(app))
                .returns(() => of(new RulesDto([rule1, rule2]))).verifiable();

            rulesState.load().subscribe();
        });

        it('should add rule to snapshot when created', () => {
            const request = { trigger: { triggerType: 'trigger3', value: 3 }, action: { actionType: 'action3', value: 1 } };

            rulesService.setup(x => x.postRule(app, request))
                .returns(() => of(newRule));

            rulesState.create(request).subscribe();

            expect(rulesState.snapshot.rules.values).toEqual([rule1, rule2, newRule]);
        });

        it('should update rule when updated action', () => {
            const newAction = {};

            const updated = createRule(1, 'new');

            rulesService.setup(x => x.putRule(app, rule1, It.isAny(), version))
                .returns(() => of(updated)).verifiable();

            rulesState.updateAction(rule1, newAction).subscribe();

            const newRule1 = rulesState.snapshot.rules.at(0);

            expect(newRule1).toEqual(updated);
        });

        it('should update rule when updated trigger', () => {
            const newTrigger = {};

            const updated = createRule(1, 'new');

            rulesService.setup(x => x.putRule(app, rule1, It.isAny(), version))
            .returns(() => of(updated)).verifiable();

            rulesState.updateTrigger(rule1, newTrigger).subscribe();

            const rule1New = rulesState.snapshot.rules.at(0);

            expect(rule1New).toEqual(updated);
        });

        it('should update rule when enabled', () => {
            const updated = createRule(1, 'new');

            rulesService.setup(x => x.enableRule(app, rule1, version))
            .returns(() => of(updated)).verifiable();

            rulesState.enable(rule1).subscribe();

            const rule1New = rulesState.snapshot.rules.at(0);

            expect(rule1New).toEqual(updated);
        });

        it('should update rule when disabled', () => {
            const updated = createRule(1, 'new');

            rulesService.setup(x => x.disableRule(app, rule1, version))
                .returns(() => of(updated)).verifiable();

            rulesState.disable(rule1).subscribe();

            const rule1New = rulesState.snapshot.rules.at(0);

            expect(rule1New).toEqual(updated);
        });

        it('should remove rule from snapshot when deleted', () => {
            rulesService.setup(x => x.deleteRule(app, rule1, version))
                .returns(() => of(versioned(newVersion))).verifiable();

            rulesState.delete(rule1).subscribe();

            expect(rulesState.snapshot.rules.values).toEqual([rule2]);
        });
    });
});