/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DialogService, RulesDto, RulesService, versioned } from '@app/shared/internal';
import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';
import { RuleDto } from './../services/rules.service';
import { createRule } from './../services/rules.service.spec';
import { RulesState } from './rules.state';
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
                .returns(() => of(new RulesDto([rule1, rule2], {}, rule1.id))).verifiable();

            rulesState.load().subscribe();

            expect(rulesState.snapshot.isLoaded).toBeTruthy();
            expect(rulesState.snapshot.isLoading).toBeFalsy();
            expect(rulesState.snapshot.rules).toEqual([rule1, rule2]);

            let runningRule: RuleDto | undefined;

            rulesState.runningRule.subscribe(result => {
                runningRule = result;
            });

            expect(runningRule).toBe(rule1);
            expect(rulesState.snapshot.runningRuleId).toBe(rule1.id);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading if loading failed', () => {
            rulesService.setup(x => x.getRules(app))
                .returns(() => throwError('error'));

            rulesState.load().pipe(onErrorResumeNext()).subscribe();

            expect(rulesState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            rulesService.setup(x => x.getRules(app))
                .returns(() => of(new RulesDto([rule1, rule2]))).verifiable();

            rulesState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should replace selected rule if reloading', () => {
            const newRules = [
                createRule(1, '_new'),
                createRule(2, '_new')
            ];

            rulesService.setup(x => x.getRules(app))
                .returns(() => of(new RulesDto([rule1, rule2]))).verifiable(Times.exactly(2));

            rulesService.setup(x => x.getRules(app))
                .returns(() => of(new RulesDto(newRules)));

            rulesState.load().subscribe();
            rulesState.select(rule1.id).subscribe();
            rulesState.load().subscribe();

            expect(rulesState.snapshot.selectedRule).toEqual(newRules[0]);
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            rulesService.setup(x => x.getRules(app))
                .returns(() => of(new RulesDto([rule1, rule2]))).verifiable();

            rulesState.load().subscribe();
        });

        it('should return rule on select and not load if already loaded', () => {
            let selectedRule: RuleDto;

            rulesState.select(rule1.id).subscribe(x => {
                selectedRule = x!;
            });

            expect(selectedRule!).toEqual(rule1);
            expect(rulesState.snapshot.selectedRule).toEqual(rule1);
        });

        it('should return null on select if unselecting rule', () => {
            let selectedRule: RuleDto;

            rulesState.select(null).subscribe(x => {
                selectedRule = x!;
            });

            expect(selectedRule!).toBeNull();
            expect(rulesState.snapshot.selectedRule).toBeNull();
        });

        it('should add rule to snapshot if created', () => {
            const request = { trigger: { triggerType: 'trigger3', value: 3 }, action: { actionType: 'action3', value: 1 } };

            rulesService.setup(x => x.postRule(app, request))
                .returns(() => of(newRule));

            rulesState.create(request).subscribe();

            expect(rulesState.snapshot.rules).toEqual([rule1, rule2, newRule]);
        });

        it('should update rule if updated', () => {
            const request = {};

            const updated = createRule(1, '_new');

            rulesService.setup(x => x.putRule(app, rule1, It.isAny(), version))
                .returns(() => of(updated)).verifiable();

            rulesState.update(rule1, request).subscribe();

            const newRule1 = rulesState.snapshot.rules[0];

            expect(newRule1).toEqual(updated);
        });

        it('should update rule if renamed', () => {
            const newName = 'NewName';

            const updated = createRule(1, '_new');

            rulesService.setup(x => x.putRule(app, rule1, It.isAny(), version))
                .returns(() => of(updated)).verifiable();

            rulesState.rename(rule1, newName).subscribe();

            const rule1New = rulesState.snapshot.rules[0];

            expect(rule1New).toEqual(updated);
        });

        it('should update rule if enabled', () => {
            const updated = createRule(1, '_new');

            rulesService.setup(x => x.enableRule(app, rule1, version))
                .returns(() => of(updated)).verifiable();

            rulesState.enable(rule1).subscribe();

            const rule1New = rulesState.snapshot.rules[0];

            expect(rule1New).toEqual(updated);
        });

        it('should not update rule if triggered', () => {
            rulesService.setup(x => x.triggerRule(app, rule1))
                .returns(of).verifiable();

            rulesState.trigger(rule1).subscribe();

            const rule1New = rulesState.snapshot.rules[0];

            expect(rule1New).toEqual(rule1);
        });

        it('should not update rule if running', () => {
            rulesService.setup(x => x.runRule(app, rule1))
                .returns(of).verifiable();

            rulesState.run(rule1).subscribe();

            const rule1New = rulesState.snapshot.rules[0];

            expect(rule1New).toEqual(rule1);
        });

        it('should not update rule if running from snapshots', () => {
            rulesService.setup(x => x.runRuleFromSnapshots(app, rule1))
                .returns(of).verifiable();

            rulesState.runFromSnapshots(rule1).subscribe();

            const rule1New = rulesState.snapshot.rules[0];

            expect(rule1New).toEqual(rule1);
        });

        it('should update rule if disabled', () => {
            const updated = createRule(1, '_new');

            rulesService.setup(x => x.disableRule(app, rule1, version))
                .returns(() => of(updated)).verifiable();

            rulesState.disable(rule1).subscribe();

            const rule1New = rulesState.snapshot.rules[0];

            expect(rule1New).toEqual(updated);
        });

        it('should remove rule from snapshot if deleted', () => {
            rulesService.setup(x => x.deleteRule(app, rule1, version))
                .returns(() => of(versioned(newVersion))).verifiable();

            rulesState.delete(rule1).subscribe();

            expect(rulesState.snapshot.rules).toEqual([rule2]);
        });

        it('should invoke rule service if run is cancelled', () => {
            rulesService.setup(x => x.runCancel(app))
                .returns(of).verifiable();

            rulesState.runCancel().subscribe();

            expect().nothing();
        });
    });
});