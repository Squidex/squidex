/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { firstValueFrom, of, onErrorResumeNextWith, throwError } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { DialogService, DynamicCreateRuleDto, DynamicFlowDefinitionDto, DynamicFlowStepDefinitionDto, DynamicRuleDto, DynamicRulesDto, DynamicUpdateRuleDto, ManualRuleTriggerDto, RulesService, Types, versioned } from '@app/shared/internal';
import { createRule } from '../services/rules.service.spec';
import { TestValues } from './_test-helpers';
import { FlowView, RulesState } from './rules.state';

describe('RulesState', () => {
    const { app, appsState, newVersion, } = TestValues;

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
                .returns(() => of(new DynamicRulesDto(({ items: [rule1, rule2], runningRuleId: rule1.id, _links: {} })))).verifiable();

            rulesState.load().subscribe();

            expect(rulesState.snapshot.isLoaded).toBeTruthy();
            expect(rulesState.snapshot.isLoading).toBeFalsy();
            expect(rulesState.snapshot.rules).toEqual([rule1, rule2]);

            let ruleRunning: DynamicRuleDto | undefined;
            rulesState.runningRule.subscribe(result => {
                ruleRunning = result;
            });

            expect(ruleRunning).toBe(rule1);
            expect(rulesState.snapshot.runningRuleId).toBe(rule1.id);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading state if loading failed', () => {
            rulesService.setup(x => x.getRules(app))
                .returns(() => throwError(() => 'Service Error'));

            rulesState.load().pipe(onErrorResumeNextWith()).subscribe();

            expect(rulesState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            rulesService.setup(x => x.getRules(app))
                .returns(() => of(new DynamicRulesDto(({ items: [rule1, rule2], _links: {} })))).verifiable();

            rulesState.load(true).subscribe();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            rulesService.setup(x => x.getRules(app))
                .returns(() => of(new DynamicRulesDto(({ items: [rule1, rule2], _links: {} })))).verifiable();

            rulesState.load().subscribe();
        });

        it('should return rule on select and not load if already loaded', async () => {
            const ruleSelected = await firstValueFrom(rulesState.select(rule1.id));

            expect(ruleSelected).toEqual(rule1);
            expect(rulesState.snapshot.selectedRule).toEqual(rule1);
        });

        it('should return null on select if unselecting rule', async () => {
            const ruleSelected = await firstValueFrom(rulesState.select(null));

            expect(ruleSelected).toBeNull();
            expect(rulesState.snapshot.selectedRule).toBeNull();
        });

        it('should add rule to snapshot if created', () => {
            const request = new DynamicCreateRuleDto({
                trigger: new ManualRuleTriggerDto(),
                action: {
                    actionType: 'action3',
                },
                flow: new DynamicFlowDefinitionDto(),
            });

            rulesService.setup(x => x.postRule(app, request))
                .returns(() => of(newRule));

            rulesState.create(request).subscribe();

            expect(rulesState.snapshot.rules).toEqual([rule1, rule2, newRule]);
        });

        it('should update rule if updated', () => {
            const request = new DynamicUpdateRuleDto({
                trigger: new ManualRuleTriggerDto(),
                action: {
                    actionType: 'action3',
                },
            });

            const updated = createRule(1, '_new');

            rulesService.setup(x => x.putRule(app, rule1, request, rule1.version))
                .returns(() => of(updated)).verifiable();

            rulesState.update(rule1, request).subscribe();

            expect(rulesState.snapshot.rules).toEqual([updated, rule2]);
        });

        it('should not update rule in snapshot if triggered', () => {
            rulesService.setup(x => x.triggerRule(app, rule1))
                .returns(() => of(true)).verifiable();

            rulesState.trigger(rule1).subscribe();

            expect(rulesState.snapshot.rules).toEqual([rule1, rule2]);
        });

        it('should not update rule in snapshot if running', () => {
            rulesService.setup(x => x.runRule(app, rule1))
                .returns(() => of(true)).verifiable();

            rulesState.run(rule1).subscribe();

            expect(rulesState.snapshot.rules).toEqual([rule1, rule2]);
        });

        it('should not update rule in snapshot if running from snapshots', () => {
            rulesService.setup(x => x.runRuleFromSnapshots(app, rule1))
                .returns(() => of(true)).verifiable();

            rulesState.runFromSnapshots(rule1).subscribe();

            expect(rulesState.snapshot.rules).toEqual([rule1, rule2]);
        });

        it('should remove rule from snapshot if deleted', () => {
            rulesService.setup(x => x.deleteRule(app, rule1, rule1.version))
                .returns(() => of(versioned(newVersion))).verifiable();

            rulesState.delete(rule1).subscribe();

            expect(rulesState.snapshot.rules).toEqual([rule2]);
        });

        it('should invoke rule service if run is cancelled', () => {
            rulesService.setup(x => x.runCancel(app))
                .returns(() => of(true)).verifiable();

            rulesState.runCancel().subscribe();
        });
    });

    describe('Selection', () => {
        beforeEach(() => {
            rulesService.setup(x => x.getRules(app))
                .returns(() => of(new DynamicRulesDto(({ items: [rule1, rule2], _links: {} })))).verifiable(Times.atLeastOnce());

            rulesState.load().subscribe();
            rulesState.select(rule2.id).subscribe();
        });

        it('should update selected rule if reloaded', () => {
            const newRules = [
                createRule(1, '_new'),
                createRule(2, '_new'),
            ];

            rulesService.setup(x => x.getRules(app))
                .returns(() => of(new DynamicRulesDto(({ items: newRules, _links: {} })))).verifiable(Times.exactly(2));

            rulesState.load().subscribe();

            expect(rulesState.snapshot.selectedRule).toEqual(newRules[1]);
        });

        it('should update selected rule if updated', () => {
            const request = new DynamicUpdateRuleDto({
                trigger: new ManualRuleTriggerDto(),
                action: {
                    actionType: 'action3',
                },
            });

            const updated = createRule(2, '_new');

            rulesService.setup(x => x.putRule(app, rule2, request, rule2.version))
                .returns(() => of(updated)).verifiable();

            rulesState.update(rule2, request).subscribe();

            expect(rulesState.snapshot.selectedRule).toEqual(updated);
        });

        it('should remove selected rule from snapshot if deleted', () => {
            rulesService.setup(x => x.deleteRule(app, rule2, rule2.version))
                .returns(() => of(versioned(newVersion))).verifiable();

            rulesState.delete(rule2).subscribe();

            expect(rulesState.snapshot.selectedRule).toBeNull();
        });
    });
});

describe('FlowView', () => {
    const dto = new DynamicFlowDefinitionDto({ initialStepId: null!, steps: {} });
    let idCounter = 0;
    let idGenerator = () => `${idCounter++}`;

    beforeEach(() => {
        idCounter = 1;
    });

    it('should create empty flow', () => {
        const flow = new FlowView(dto);

        expect(cleanup(flow.dto.toJSON())).toEqual({
            initialStepId: null!,
            steps: {},
        });
    });

    it('should add step to flow', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Action' } });

        expect(cleanup(flow.dto.toJSON())).toEqual({
            initialStepId: '1',
            steps: {
                '1': { step: { stepType: 'Action' } },
            },
        });
    });

    it('should add second step to flow after step', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Action1' } })
            .add({ step: { stepType: 'Action2' } }, '1');

        expect(cleanup(flow.dto.toJSON())).toEqual({
            initialStepId: '1',
            steps: {
                '1': { step: { stepType: 'Action1' }, nextStepId: '2' },
                '2': { step: { stepType: 'Action2' } },
            },
        });
    });

    it('should add second step to flow', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Action1' } })
            .add({ step: { stepType: 'Action2' } });

        expect(cleanup(flow.dto.toJSON())).toEqual({
            initialStepId: '2',
            steps: {
                '1': { step: { stepType: 'Action1' } },
                '2': { step: { stepType: 'Action2' }, nextStepId: '1' },
            },
        });
    });

    it('should add step to if-else', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'If', branches: [] } })
            .add({ step: { stepType: 'Action' } }, undefined, '1');

        expect(cleanup(flow.dto.toJSON())).toEqual({
            initialStepId: '1',
            steps: {
                '1': { step: { stepType: 'If', branches: [], elseStepId: '2' } },
                '2': { step: { stepType: 'Action' } },
            },
        });
    });

    it('should add step to If-branch', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({
            step: {
                stepType: 'If',
                branches: [
                    { condition: 'Condition1' },
                    { condition: 'Condition2' },
                ],
            },
        })
            .add({ step: { stepType: 'Action' } }, undefined, '1', 1);

        expect(cleanup(flow.dto.toJSON())).toEqual({
            initialStepId: '1',
            steps: {
                '1': {
                    step: {
                        stepType: 'If',
                        branches: [
                            { condition: 'Condition1' },
                            { condition: 'Condition2', nextStepId: '2' },
                        ],
                    },
                },
                '2': { step: { stepType: 'Action' } },
            },
        });
    });

    it('should add If-step to middle', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Action' } })
            .add({ step: { stepType: 'Action' } }, '1')
            .add({ step: { stepType: 'If' } }, '1');

        expect(cleanup(flow.dto.toJSON())).toEqual({
            initialStepId: '1',
            steps: {
                '1': { step: { stepType: 'Action' }, nextStepId: '3' },
                '3': { step: { stepType: 'If' } },
            },
        });
    });

    it('should return same flow if step to add after does not exist', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Action1' } });

        const updated = flow.add({ step: { stepType: 'Action2' } }, '3');

        expect(updated).toBe(flow);
    });

    it('should return same flow if step to add to does not exist', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Action1' } });

        const updated = flow.add({ step: { stepType: 'Action2' } }, undefined, '3');

        expect(updated).toBe(flow);
    });

    it('should return same flow if step to add is not an If-step', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Action1' } });

        const updated = flow.add({ step: { stepType: 'Action2' } }, undefined, '1');

        expect(updated).toBe(flow);
    });

    it('should return same flow if step to add has no matching branch', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'If', branches: [] } });

        const updated = flow.add({ step: { stepType: 'Action2' } }, undefined, '1', 2);

        expect(updated).toBe(flow);
    });

    it('should return same flow if step to add is adder after If-step', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'If', branches: [] } });

        const updated = flow.add({ step: { stepType: 'Action2' } }, '1');

        expect(updated).toBe(flow);
    });

    it('should remove only start step', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Action' } })
            .remove('1');

        expect(cleanup(flow.dto.toJSON())).toEqual({
            initialStepId: null,
            steps: {},
        });
    });

    it('should remove start step', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Action1' } })
            .add({ step: { stepType: 'Action2' } }, '1')
            .remove('1');

        expect(cleanup(flow.dto.toJSON())).toEqual({
            initialStepId: '2',
            steps: {
                '2': { step: { stepType: 'Action2' } },
            },
        });
    });

    it('should remove last step', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Action1' } })
            .add({ step: { stepType: 'Action2' } }, '1')
            .remove('2');

        expect(cleanup(flow.dto.toJSON())).toEqual({
            initialStepId: '1',
            steps: {
                '1': { step: { stepType: 'Action1' } },
            },
        });
    });

    it('should remove middle step', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Action1' } })
            .add({ step: { stepType: 'Action2' } }, '1')
            .add({ step: { stepType: 'Action3' } }, '2')
            .remove('2');

        expect(cleanup(flow.dto.toJSON())).toEqual({
            initialStepId: '1',
            steps: {
                '1': { step: { stepType: 'Action1' }, nextStepId: '3' },
                '3': { step: { stepType: 'Action3' } },
            },
        });
    });

    it('should remove from if-else', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'If', branches: [{}] } })
            .add({ step: { stepType: 'Action' } }, undefined, '1', 1)
            .remove('2', '1', 1);

        expect(cleanup(flow.dto.toJSON())).toEqual({
            initialStepId: '1',
            steps: {
                '1': { step: { stepType: 'If', branches: [{}] } },
            },
        });
    });

    it('should remove from If-branch', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'If', branches: [{}] } })
            .add({ step: { stepType: 'Action' } }, undefined, '1')
            .remove('2', '1');

        expect(cleanup(flow.dto.toJSON())).toEqual({
            initialStepId: '1',
            steps: {
                '1': { step: { stepType: 'If', branches: [{}] } },
            },
        });
    });

    it('should remove If-step', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Action0' } })
            .add({
            step: {
                stepType: 'If',
                branches: [
                    { condition: 'Condition1' },
                    { condition: 'Condition2' },
                ],
            },
        }, '1')
            .add({ step: { stepType: 'Action1' } }, undefined, '2', 0)
            .add({ step: { stepType: 'Action2' } }, undefined, '2', 1)
            .add({ step: { stepType: 'Action3' } }, undefined, '2', 2)
            .remove('2');

        expect(cleanup(flow.dto.toJSON())).toEqual({
            initialStepId: '1',
            steps: {
                '1': { step: { stepType: 'Action0' } },
            },
        });
    });

    it('should return same flow if step not found', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Action1' } });

        const updated = flow.remove('3');

        expect(updated).toBe(flow);
    });

    it('should return same flow if step to remove from does not exist', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Action1' } });

        const updated = flow.remove('1', '3');

        expect(updated).toBe(flow);
    });

    it('should return same flow if step to remove from is not an If-step', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Action1' } })
            .add({ step: { stepType: 'Action2' } }, '1');

        const updated = flow.remove('1', '2');

        expect(updated).toBe(flow);
    });

    it('should return same flow if step to remove from has no matching branch', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'If', branches: [] } })
            .add({ step: { stepType: 'Action2' } }, '1');

        const updated = flow.remove('1', '2', 2);

        expect(updated).toBe(flow);
    });

    it('should return same flow if step to remove from has no child', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'If', branches: [] } })
            .add({ step: { stepType: 'Action2' } }, '1');

        const updated = flow.remove('1', '2', 0);

        expect(updated).toBe(flow);
    });

    it('should update step', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Delay1' } })
            .add({ step: { stepType: 'Delay2' } }, '1')
            .update('1', { step: { stepType: 'Delay1_2' }, ignoreError: true });

        expect(cleanup(flow.dto.toJSON())).toEqual({
            initialStepId: '1',
            steps: {
                '1': { step: { stepType: 'Delay1_2' }, ignoreError: true, nextStepId: '2' },
                '2': { step: { stepType: 'Delay2' } },
            },
        });
    });

    it('should return same flow if step to update does not exist', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Delay1' } })
            .add({ step: { stepType: 'Delay2' } }, '1');

        const updated = flow.update('3', { step: { stepType: 'Delay3' } });

        expect(updated).toBe(flow);
    });

    it('should get branches', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Action0' } })
            .add({
            step: {
                stepType: 'If',
                branches: [
                    { condition: 'Condition1' },
                    { condition: 'Condition2' },
                ],
            },
        }, '1')
            .add({ step: { stepType: 'Action1' } }, undefined, '2', 0)
            .add({ step: { stepType: 'Action2' } }, undefined, '2', 1)
            .add({ step: { stepType: 'Action3' } }, undefined, '2', 2);

        expect(cleanup(flow.getBranches()) as any).toEqual([
            {
                label: 'root',
                rootId: '1',
                items: [
                    {
                        id: '1',
                        step: new DynamicFlowStepDefinitionDto({
                            step: {
                                stepType: 'Action0',
                            },
                            nextStepId: '2',
                        }),
                    },
                    {
                        id: '2',
                        step: new DynamicFlowStepDefinitionDto({
                            step: {
                                stepType: 'If',
                                branches: [
                                    { condition: 'Condition1', nextStepId: '3' },
                                    { condition: 'Condition2', nextStepId: '4' },
                                ],
                                elseStepId: '5',
                            },
                        }),
                    },
                ],
            },
        ]);

        expect(cleanup(flow.getBranches('2')) as any).toEqual([
            {
                label: 'if: Condition1',
                rootId: '3',
                items: [
                    {
                        id: '3',
                        step: new DynamicFlowStepDefinitionDto({
                            step: {
                                stepType: 'Action1',
                            },
                        }),
                    },
                ],
            },
            {
                label: 'if: Condition2',
                rootId: '4',
                items: [
                    {
                        id: '4',
                        step: new DynamicFlowStepDefinitionDto({
                            step: {
                                stepType: 'Action2',
                            },
                        }),
                    },
                ],
            },
            {
                label: 'else',
                rootId: '5',
                items: [
                    {
                        id: '5',
                        step: new DynamicFlowStepDefinitionDto({
                            step: {
                                stepType: 'Action3',
                            },
                        }),
                    },
                ],
            },
        ]);
    });

    it('should get flat items', () => {
        const flow = new FlowView(dto, idGenerator)
            .add({ step: { stepType: 'Action0' } })
            .add({
            step: {
                stepType: 'If',
                branches: [
                    { condition: 'Condition1' },
                    { condition: 'Condition2' },
                ],
            },
        }, '1')
            .add({ step: { stepType: 'Action1' } }, undefined, '2', 0)
            .add({ step: { stepType: 'Action2' } }, undefined, '2', 1)
            .add({ step: { stepType: 'Action3' } }, undefined, '2', 2);

        expect(cleanup(flow.getAllItems())).toEqual([
            {
                id: '1',
                step: new DynamicFlowStepDefinitionDto({
                    step: {
                        stepType: 'Action0',
                    },
                    nextStepId: '2',
                }),
            },
            {
                id: '2',
                step: new DynamicFlowStepDefinitionDto({
                    step: {
                        stepType: 'If',
                        branches: [
                            { condition: 'Condition1', nextStepId: '3' },
                            { condition: 'Condition2', nextStepId: '4' },
                        ],
                        elseStepId: '5',
                    },
                }),
            },
            {
                id: '3',
                step: new DynamicFlowStepDefinitionDto({
                    step: {
                        stepType: 'Action1',
                    },
                }),
            },
            {
                id: '4',
                step: new DynamicFlowStepDefinitionDto({
                    step: {
                        stepType: 'Action2',
                    },
                }),
            },
            {
                id: '5',
                step: new DynamicFlowStepDefinitionDto({
                    step: {
                        stepType: 'Action3',
                    },
                }),
            },
        ]);
    });
});

function cleanup<T>(value: T) {
    if (Types.isArray(value)) {
        for (const item of value) {
            cleanup(item);
        }
    }
    else {
        for (const [key, nested] of Object.entries(value as any)) {
            if (nested === value) {
                continue;
            }

            if (key === 'initialStepId') {
                continue;
            }

            if (key === 'setRoot') {
                delete (value as any)[key];
            }

            if (Types.isNull(nested) || Types.isUndefined(nested)) {
                delete (value as any)[key];
            }
            else {
                cleanup(nested);
            }
        }
    }

    return value;
}
