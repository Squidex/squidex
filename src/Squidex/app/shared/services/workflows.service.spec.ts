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

        expect(workflow.name).toEqual('Default');
    });

    it('should add step to workflow', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00' });

        expect(simplify(workflow)).toEqual({
            _links: {},
            steps: [
                { name: '1', color: '#00ff00' }
            ],
            transitions: [],
            name: 'Default'
        });
    });

    it('should override settings if step already exists', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00' })
                .setStep('1', { color: 'red' });

        expect(simplify(workflow)).toEqual({
            _links: {},
            steps: [
                { name: '1', color: 'red' }
            ],
            transitions: [],
            name: 'Default'
        });
    });

    it('should not remove step if locked', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00', isLocked: true })
                .setStep('2', { color: '#ff0000' })
                .setTransition('1', '2', { expression: '1 === 2' })
                .removeStep('1');

        expect(simplify(workflow)).toEqual({
            _links: {},
            steps: [
                { name: '1', color: '#00ff00', isLocked: true },
                { name: '2', color: '#ff0000' }
            ],
            transitions: [
                { from: '1', to: '2', expression: '1 === 2' }
            ],
            name: 'Default'
        });
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

        expect(simplify(workflow)).toEqual({
            _links: {},
            steps: [
                { name: '2', color: '#ff0000' },
                { name: '3', color: '#0000ff' }
            ],
            transitions: [
                { from: '2', to: '3', expression: '2 === 3' }
            ],
            name: 'Default'
        });
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
                .renameStep('1', '4');

        expect(simplify(workflow)).toEqual({
            _links: {},
            steps: [
                { name: '4', color: '#00ff00' },
                { name: '2', color: '#ff0000' },
                { name: '3', color: '#0000ff' }
            ],
            transitions: [
                { from: '4', to: '2', expression: '1 === 2' },
                { from: '2', to: '4', expression: '2 === 1' },
                { from: '2', to: '3', expression: '2 === 3' }
            ],
            name: 'Default'
        });
    });

    it('should add transitions to workflow', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00' })
                .setStep('2', { color: '#ff0000' })
                .setTransition('1', '2', { expression: '1 === 2' })
                .setTransition('2', '1', { expression: '2 === 1' });

        expect(simplify(workflow)).toEqual({
            _links: {},
            steps: [
                { name: '1', color: '#00ff00' },
                { name: '2', color: '#ff0000' }
            ],
            transitions: [
                { from: '1', to: '2', expression: '1 === 2' },
                { from: '2', to: '1', expression: '2 === 1' }
            ],
            name: 'Default'
        });
    });

    it('should add remove transition from workflow', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00' })
                .setStep('2', { color: '#ff0000' })
                .setTransition('1', '2', { expression: '1 === 1' })
                .setTransition('2', '1', { expression: '2 === 1' })
                .removeTransition('1', '2');

        expect(simplify(workflow)).toEqual({
            _links: {},
            steps: [
                { name: '1', color: '#00ff00' },
                { name: '2', color: '#ff0000' }
            ],
            transitions: [
                { from: '2', to: '1', expression: '2 === 1' }
            ],
            name: 'Default'
        });
    });

    it('should override settings if transition already exists', () => {
        const workflow =
            new WorkflowDto()
                .setStep('1', { color: '#00ff00' })
                .setStep('2', { color: '#ff0000' })
                .setTransition('1', '2', { expression: '1 === 2' })
                .setTransition('1', '2', { expression: '1 !== 2' });

        expect(simplify(workflow)).toEqual({
            _links: {},
            steps: [
                { name: '1', color: '#00ff00' },
                { name: '2', color: '#ff0000' }
            ],
            transitions: [
                { from: '1', to: '2', expression: '1 !== 2' }
            ],
            name: 'Default'
        });
    });
});

function simplify(value: any) {
    return JSON.parse(JSON.stringify(value));
}