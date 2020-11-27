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

        it('should reset loading when loading failed', () => {
            rulesService.setup(x => x.getRules(app))
                .returns(() => throwError('error'));

            rulesState.load().pipe(onErrorResumeNext()).subscribe();

            expect(rulesState.snapshot.isLoading).toBeFalsy();
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

            expect(rulesState.snapshot.rules).toEqual([rule1, rule2, newRule]);
        });

        it('should update rule when updated action', () => {
            const newAction = {};

            const updated = createRule(1, '_new');

            rulesService.setup(x => x.putRule(app, rule1, It.isAny(), version))
                .returns(() => of(updated)).verifiable();

            rulesState.updateAction(rule1, newAction).subscribe();

            const newRule1 = rulesState.snapshot.rules[0];

            expect(newRule1).toEqual(updated);
        });

        it('should update rule when updated trigger', () => {
            const newTrigger = {};

            const updated = createRule(1, '_new');

            rulesService.setup(x => x.putRule(app, rule1, It.isAny(), version))
                .returns(() => of(updated)).verifiable();

            rulesState.updateTrigger(rule1, newTrigger).subscribe();

            const rule1New = rulesState.snapshot.rules[0];

            expect(rule1New).toEqual(updated);
        });

        it('should update rule when renamed', () => {
            const newName = 'NewName';

            const updated = createRule(1, '_new');

            rulesService.setup(x => x.putRule(app, rule1, It.isAny(), version))
                .returns(() => of(updated)).verifiable();

            rulesState.rename(rule1, newName).subscribe();

            const rule1New = rulesState.snapshot.rules[0];

            expect(rule1New).toEqual(updated);
        });

        it('should update rule when enabled', () => {
            const updated = createRule(1, '_new');

            rulesService.setup(x => x.enableRule(app, rule1, version))
                .returns(() => of(updated)).verifiable();

            rulesState.enable(rule1).subscribe();

            const rule1New = rulesState.snapshot.rules[0];

            expect(rule1New).toEqual(updated);
        });

        it('should not update rule when triggered', () => {
            rulesService.setup(x => x.triggerRule(app, rule1))
                .returns(() => of()).verifiable();

            rulesState.trigger(rule1).subscribe();

            const rule1New = rulesState.snapshot.rules[0];

            expect(rule1New).toEqual(rule1);
        });

        it('should not update rule when rurunningn', () => {
            rulesService.setup(x => x.runRule(app, rule1))
                .returns(() => of()).verifiable();

            rulesState.run(rule1).subscribe();

            const rule1New = rulesState.snapshot.rules[0];

            expect(rule1New).toEqual(rule1);
        });

        it('should not update rule when running from snapshots', () => {
            rulesService.setup(x => x.runRuleFromSnapshots(app, rule1))
                .returns(() => of()).verifiable();

            rulesState.runFromSnapshots(rule1).subscribe();

            const rule1New = rulesState.snapshot.rules[0];

            expect(rule1New).toEqual(rule1);
        });

        it('should update rule when disabled', () => {
            const updated = createRule(1, '_new');

            rulesService.setup(x => x.disableRule(app, rule1, version))
                .returns(() => of(updated)).verifiable();

            rulesState.disable(rule1).subscribe();

            const rule1New = rulesState.snapshot.rules[0];

            expect(rule1New).toEqual(updated);
        });

        it('should remove rule from snapshot when deleted', () => {
            rulesService.setup(x => x.deleteRule(app, rule1, version))
                .returns(() => of(versioned(newVersion))).verifiable();

            rulesState.delete(rule1).subscribe();

            expect(rulesState.snapshot.rules).toEqual([rule2]);
        });

        it('should invoke rule service when run is cancelled', () => {
            rulesService.setup(x => x.runCancel(app))
                .returns(() => of()).verifiable();

            rulesState.runCancel().subscribe();

            expect().nothing();
        });
    });
});