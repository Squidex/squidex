/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { WorkflowDto } from '@app/shared/internal';

describe('Workflow', () => {
    it('should create empty workflow', () => {
        const workflow = new WorkflowDto();

        expect(workflow.initial);
    });

    it('should add step to workflow', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00' });

        expect(workflow.serialize()).toEqual({
            steps: {
                '1': { transitions: {}, color: '#00ff00' }
            },
            initial: '1'
        });
    });

    it('should override settings if step already exists', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00', noUpdate: true })
                .setStep('1', { color: 'red' });

        expect(workflow.serialize()).toEqual({
            steps: {
                '1': { transitions: {}, color: 'red', noUpdate: true }
            },
            initial: '1'
        });
    });

    it('should return same workflow if step to update is locked', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00', isLocked: true });

        const updated = workflow.setStep('1', { color: 'red' });

        expect(updated).toBe(workflow);
    });

    it('should sort steps case invariant', () => {
        const workflow =
            new WorkflowDto()
                .setStep('Z')
                .setStep('a');

        expect(workflow.steps).toEqual([
            { name: 'a' },
            { name: 'Z' }
        ]);
    });

    it('should return same workflow if step to remove is locked', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00', isLocked: true });

        const updated = workflow.removeStep('1');

        expect(updated).toBe(workflow);
    });

    it('should return same workflow if step to remove not found', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1');

        const updated = workflow.removeStep('3');

        expect(updated).toBe(workflow);
    });

    it('should remove step', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00' })
                .setStep('2', { color: '#ff0000' })
                .setStep('3', { color: '#0000ff' })
                .setTransition('1', '2', { expression: '1 === 2' })
                .setTransition('1', '3', { expression: '1 === 3' })
                .setTransition('2', '3', { expression: '2 === 3' })
                .removeStep('1');

        expect(workflow.serialize()).toEqual({
            steps: {
                '2': {
                    transitions: {
                        '3': { expression: '2 === 3' }
                    },
                    color: '#ff0000'
                },
                '3': { transitions: {}, color: '#0000ff' }
            },
            initial: '2'
        });
    });

    it('should make first non-locked step the initial step if initial removed', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2', { isLocked: true })
                .setStep('3')
                .removeStep('1');

        expect(workflow.serialize()).toEqual({
            steps: {
                '2': { transitions: {}, isLocked: true },
                '3': { transitions: {} }
            },
            initial: '3'
        });
    });

    it('should unset initial step if initial removed', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .removeStep('1');

        expect(workflow.serialize()).toEqual({ steps: {}, initial: undefined });
    });

    it('should rename step', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00' })
                .setStep('2', { color: '#ff0000' })
                .setStep('3', { color: '#0000ff' })
                .setTransition('1', '2', { expression: '1 === 2' })
                .setTransition('2', '1', { expression: '2 === 1' })
                .setTransition('2', '3', { expression: '2 === 3' })
                .renameStep('1', 'a');

        expect(workflow.serialize()).toEqual({
            steps: {
                'a': {
                    transitions: {
                        '2': { expression: '1 === 2' }
                    },
                    color: '#00ff00'
                },
                '2': {
                    transitions: {
                        'a': { expression: '2 === 1' },
                        '3': { expression: '2 === 3' }
                    },
                    color: '#ff0000'
                },
                '3': { transitions: {}, color: '#0000ff' }
            },
            initial: 'a'
        });
    });

    it('should add transitions to workflow', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2')
                .setTransition('1', '2', { expression: '1 === 2' })
                .setTransition('2', '1', { expression: '2 === 1' });

        expect(workflow.serialize()).toEqual({
            steps: {
                '1': {
                    transitions: {
                        '2': { expression: '1 === 2' }
                    }
                },
                '2': {
                    transitions: {
                        '1': { expression: '2 === 1' }
                    }
                }
            },
            initial: '1'
        });
    });

    it('should remove transition from workflow', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2')
                .setTransition('1', '2', { expression: '1 === 2' })
                .setTransition('2', '1', { expression: '2 === 1' })
                .removeTransition('1', '2');

        expect(workflow.serialize()).toEqual({
            steps: {
                '1': { transitions: {}},
                '2': {
                    transitions: {
                        '1': { expression: '2 === 1' }
                    }
                }
            },
            initial: '1'
        });
    });

    it('should override settings if transition already exists', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2')
                .setTransition('2', '1', { expression: '2 === 1', role: 'Role' })
                .setTransition('2', '1', { expression: '2 !== 1' });

        expect(workflow.serialize()).toEqual({
            steps: {
                '1': { transitions: {} },
                '2': {
                    transitions: {
                        '1': { expression: '2 !== 1', role: 'Role' }
                    }
                }
            },
            initial: '1'
        });
    });

    it('should return same workflow if transition to update not found by from step', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2')
                .setTransition('1', '2');

        const updated = workflow.setTransition('3', '2', { role: 'Role' });

        expect(updated).toBe(workflow);
    });

    it('should return same workflow if transition to update not found by to step', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2')
                .setTransition('1', '2');

        const updated = workflow.setTransition('1', '3', { role: 'Role' });

        expect(updated).toBe(workflow);
    });

    it('should return same workflow if transition to remove not', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2')
                .setTransition('1', '2');

        const updated = workflow.removeTransition('1', '3');

        expect(updated).toBe(workflow);
    });

    it('should return same workflow if step to make initial is locked', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2', { color: '#00ff00', isLocked: true });

        const updated = workflow.setInitial('2');

        expect(updated).toBe(workflow);
    });

    it('should set initial step', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1')
                .setStep('2')
                .setInitial('2');

        expect(workflow.serialize()).toEqual({
            steps: {
                '1': { transitions: {} },
                '2': { transitions: {} }
            },
            initial: '2'
        });
    });
});