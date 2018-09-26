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
    AppsState,
    AuthService,
    CreateRuleDto,
    DateTime,
    DialogService,
    RuleDto,
    RulesService,
    UpdateRuleDto,
    Version,
    Versioned
} from '@app/shared';

describe('RulesState', () => {
    const app = 'my-app';
    const creation = DateTime.today();
    const creator = 'not-me';
    const modified = DateTime.now();
    const modifier = 'me';
    const version = new Version('1');
    const newVersion = new Version('2');

    const oldRules = [
        new RuleDto('id1', creator, creator, creation, creation, version, false, {}, 'trigger1', {}, 'action1'),
        new RuleDto('id2', creator, creator, creation, creation, version, true, {}, 'trigger2', {}, 'action2')
    ];

    let dialogs: IMock<DialogService>;
    let appsState: IMock<AppsState>;
    let authService: IMock<AuthService>;
    let rulesService: IMock<RulesService>;
    let rulesState: RulesState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        authService = Mock.ofType<AuthService>();

        authService.setup(x => x.user)
            .returns(() => <any>{ id: '1', token: modifier });

        appsState = Mock.ofType<AppsState>();

        appsState.setup(x => x.appName)
            .returns(() => app);

        rulesService = Mock.ofType<RulesService>();

        rulesService.setup(x => x.getRules(app))
            .returns(() => of(oldRules));

        rulesState = new RulesState(appsState.object, authService.object, dialogs.object, rulesService.object);
        rulesState.load().subscribe();
    });

    it('should load rules', () => {
        expect(rulesState.snapshot.rules.values).toEqual(oldRules);
        expect(rulesState.snapshot.isLoaded).toBeTruthy();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
    });

    it('should show notification on load when reload is true', () => {
        rulesState.load(true).subscribe();

        expect().nothing();

        dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
    });

    it('should add rule to snapshot when created', () => {
        const newRule = new RuleDto('id3', creator, creator, creation, creation, version, false, {}, 'trigger3', {}, 'action3');

        const request = new CreateRuleDto({}, {});

        rulesService.setup(x => x.postRule(app, request, modifier, creation))
            .returns(() => of(newRule));

        rulesState.create(request, creation).subscribe();

        expect(rulesState.snapshot.rules.values).toEqual([...oldRules, newRule]);
    });

    it('should update action and update and user info when updated action', () => {
        const newAction = {};

        rulesService.setup(x => x.putRule(app, oldRules[0].id, It.is<UpdateRuleDto>(i => true), version))
            .returns(() => of(new Versioned<any>(newVersion, {})));

        rulesState.updateAction(oldRules[0], newAction, modified).subscribe();

        const rule_1 = rulesState.snapshot.rules.at(0);

        expect(rule_1.action).toBe(newAction);
        expectToBeModified(rule_1);
    });

    it('should update trigger and update and user info when updated trigger', () => {
        const newTrigger = {};

        rulesService.setup(x => x.putRule(app, oldRules[0].id, It.is<UpdateRuleDto>(i => true), version))
            .returns(() => of(new Versioned<any>(newVersion, {})));

        rulesState.updateTrigger(oldRules[0], newTrigger, modified).subscribe();

        const rule_1 = rulesState.snapshot.rules.at(0);

        expect(rule_1.trigger).toBe(newTrigger);
        expectToBeModified(rule_1);
    });

    it('should mark as enabled and update and user info when enabled', () => {
        rulesService.setup(x => x.enableRule(app, oldRules[0].id, version))
            .returns(() => of(new Versioned<any>(newVersion, {})));

        rulesState.enable(oldRules[0], modified).subscribe();

        const rule_1 = rulesState.snapshot.rules.at(0);

        expect(rule_1.isEnabled).toBeTruthy();
        expectToBeModified(rule_1);
    });

    it('should mark as disabled and update and user info when disabled', () => {
        rulesService.setup(x => x.disableRule(app, oldRules[1].id, version))
            .returns(() => of(new Versioned<any>(newVersion, {})));

        rulesState.disable(oldRules[1], modified).subscribe();

        const rule_1 = rulesState.snapshot.rules.at(1);

        expect(rule_1.isEnabled).toBeFalsy();
        expectToBeModified(rule_1);
    });

    it('should remove rule from snapshot when deleted', () => {
        rulesService.setup(x => x.deleteRule(app, oldRules[0].id, version))
            .returns(() => of(new Versioned<any>(newVersion, {})));

        rulesState.delete(oldRules[0]).subscribe();

        expect(rulesState.snapshot.rules.values).toEqual([oldRules[1]]);
    });

    function expectToBeModified(rule_1: RuleDto) {
        expect(rule_1.lastModified).toEqual(modified);
        expect(rule_1.lastModifiedBy).toEqual(modifier);
        expect(rule_1.version).toEqual(newVersion);
    }
});