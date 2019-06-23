/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import {
    compareStringsAsc,
    Model,
    ResourceLinks
} from '@app/framework';

export class WorkflowDto extends Model<WorkflowDto> {
    public readonly _links: ResourceLinks;

    constructor(links: ResourceLinks = {},
        public readonly name: string = 'Default',
        public readonly steps: WorkflowStep[] = [],
        public readonly transitions: WorkflowTransition[] = []
    ) {
        super();

        this._links = links;
    }

    public onCloned() {
        this.steps.sort((a, b) => compareStringsAsc(a.name, b.name));

        this.transitions.sort((a, b) => compareStringsAsc(a.to, b.to));
    }

    public getOpenSteps(step: WorkflowStep) {
        return this.steps.filter(x => x.name !== step.name && !this.transitions.find(y => y.from === step.name && y.to === x.name));
    }

    public getTransitions(step: WorkflowStep): WorkflowTransitionView[] {
        return this.transitions.filter(x => x.from === step.name).map(x => ({ step: this.getStep(x.to), ...x }));
    }

    public getStep(name: string): WorkflowStep {
        return this.steps.find(x => x.name === name)!;
    }

    public setStep(name: string, values: Partial<WorkflowStepValues>) {
        const found = this.getStep(name);

        if (found) {
            const { name: _, ...existing } = found;

            if (found.isLocked) {
                return this;
            }

            values = { ...values, ...existing };
        }

        const steps = [...this.steps.filter(s => s !== found), { name, ...values }];

        return this.with({ steps });
    }

    public removeStep(name: string) {
        const steps = this.steps.filter(s => s.name !== name || s.isLocked);

        const transitions =
            steps.length !== this.steps.length ?
            this.transitions.filter(t => t.from !== name && t.to !== name) :
            this.transitions;

        return this.with({ steps, transitions });
    }

    public renameStep(name: string, newName: string) {
        const steps = this.steps.map(step => {
            if (step.name === name) {
                return { ...step, name: newName };
            }

            return step;
        });

        const transitions = this.transitions.map(transition => {
            if (transition.from === name || transition.to === name) {
                let newTransition = { ...transition };

                if (newTransition.from === name) {
                    newTransition.from = newName;
                }

                if (newTransition.to === name) {
                    newTransition.to = newName;
                }

                return newTransition;
            }

            return transition;
        });

        return this.with({ steps, transitions });
    }

    public removeTransition(from: string, to: string) {
        const transitions = this.transitions.filter(t => t.from !== from || t.to !== to);

        return this.with({ transitions });
    }

    public setTransition(from: string, to: string, values?: Partial<WorkflowTransitionValues>) {
        const stepFrom = this.getStep(from);

        if (!stepFrom) {
            return this;
        }

        const stepTo = this.getStep(to);

        if (!stepTo) {
            return this;
        }

        const found = this.transitions.find(x => x.from === from && x.to === to);

        if (found) {
            const { from: _, to: __, ...existing } = found;

            values = { ...values, ...existing };
        }

        const transitions = [...this.transitions.filter(t => t !== found), { from, to, ...values }];

        return this.with({ transitions });
    }
}

export type WorkflowStepValues = { color?: string; isLocked?: boolean; noUpdate?: boolean; };
export type WorkflowStep = { name: string } & WorkflowStepValues;

export type WorkflowTransitionValues = { expression?: string; role?: string; };
export type WorkflowTransition = { from: string; to: string } & WorkflowTransitionValues;

export type WorkflowTransitionView = { step: WorkflowStep } & WorkflowTransition;