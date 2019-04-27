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
    RuleDto,
    RulesService,
    Versioned
} from './../';

import { TestValues } from './_test-helpers';

describe('RulesState', () => {
    const {
        app,
        appsState,
        authService,
        creation,
        creator,
        modified,
        modifier,
        newVersion,
        version
    } = TestValues;

    const oldRules = [
        new RuleDto('id1', creator, creator, creation, creation, version, false, {}, 'trigger1', {}, 'action1'),
        new RuleDto('id2', creator, creator, creation, creation, version, true, {}, 'trigger2', {}, 'action2')
    ];

    let dialogs: IMock<DialogService>;
    let rulesService: IMock<RulesService>;
    let rulesState: RulesState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        rulesService = Mock.ofType<RulesService>();
        rulesState = new RulesState(appsState.object, authService.object, dialogs.object, rulesService.object);
    });

    afterEach(() => {
        rulesService.verifyAll();
    });

    describe('Loading', () => {
        it('should load rules', () => {
            rulesService.setup(x => x.getRules(app))
                .returns(() => of(oldRules)).verifiable();

            rulesState.load().subscribe();

            expect(rulesState.snapshot.rules.values).toEqual(oldRules);
            expect(rulesState.snapshot.isLoaded).toBeTruthy();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should show notification on load when reload is true', () => {
            rulesService.setup(x => x.getRules(app))
                .returns(() => of(oldRules)).verifiable();

            rulesState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

    });

    describe('Updates', () => {
        beforeEach(() => {
            rulesService.setup(x => x.getRules(app))
                .returns(() => of(oldRules)).verifiable();

            rulesState.load().subscribe();
        });

        it('should add rule to snapshot when created', () => {
            const newRule = new RuleDto('id3', creator, creator, creation, creation, version, false, {}, 'trigger3', {}, 'action3');

            const request = { action: {}, trigger: {} };

            rulesService.setup(x => x.postRule(app, request, modifier, creation))
                .returns(() => of(newRule));

            rulesState.create(request, creation).subscribe();

            expect(rulesState.snapshot.rules.values).toEqual([...oldRules, newRule]);
        });

        it('should update action and update and user info when updated action', () => {
            const newAction = {};

            rulesService.setup(x => x.putRule(app, oldRules[0].id, It.isAny(), version))
                .returns(() => of(new Versioned(newVersion, {}))).verifiable();

            rulesState.updateAction(oldRules[0], newAction, modified).subscribe();

            const rule_1 = rulesState.snapshot.rules.at(0);

            expect(rule_1.action).toBe(newAction);
            expectToBeModified(rule_1);
        });

        it('should update trigger and update and user info when updated trigger', () => {
            const newTrigger = {};

            rulesService.setup(x => x.putRule(app, oldRules[0].id, It.isAny(), version))
                .returns(() => of(new Versioned(newVersion, {}))).verifiable();

            rulesState.updateTrigger(oldRules[0], newTrigger, modified).subscribe();

            const rule_1 = rulesState.snapshot.rules.at(0);

            expect(rule_1.trigger).toBe(newTrigger);
            expectToBeModified(rule_1);
        });

        it('should mark as enabled and update and user info when enabled', () => {
            rulesService.setup(x => x.enableRule(app, oldRules[0].id, version))
                .returns(() => of(new Versioned(newVersion, {}))).verifiable();

            rulesState.enable(oldRules[0], modified).subscribe();

            const rule_1 = rulesState.snapshot.rules.at(0);

            expect(rule_1.isEnabled).toBeTruthy();
            expectToBeModified(rule_1);
        });

        it('should mark as disabled and update and user info when disabled', () => {
            rulesService.setup(x => x.disableRule(app, oldRules[1].id, version))
                .returns(() => of(new Versioned(newVersion, {}))).verifiable();

            rulesState.disable(oldRules[1], modified).subscribe();

            const rule_1 = rulesState.snapshot.rules.at(1);

            expect(rule_1.isEnabled).toBeFalsy();
            expectToBeModified(rule_1);
        });

        it('should remove rule from snapshot when deleted', () => {
            rulesService.setup(x => x.deleteRule(app, oldRules[0].id, version))
                .returns(() => of(new Versioned(newVersion, {}))).verifiable();

            rulesState.delete(oldRules[0]).subscribe();

            expect(rulesState.snapshot.rules.values).toEqual([oldRules[1]]);
        });

        function expectToBeModified(rule_1: RuleDto) {
            expect(rule_1.lastModified).toEqual(modified);
            expect(rule_1.lastModifiedBy).toEqual(modifier);
            expect(rule_1.version).toEqual(newVersion);
        }
    });
});