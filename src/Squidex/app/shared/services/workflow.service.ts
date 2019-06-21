/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Model, ResourceLinks } from '@app/shared';

export class WorkflowDto extends Model<WorkflowDto> {
    public readonly _links: ResourceLinks;

    constructor(links: ResourceLinks,
        public readonly name: string,
        public readonly steps: WorkflowStepDto[]
    ) {
        super();

        this._links = links;
    }

    public addStep(name: string, color: string) {
        let found = this.steps.find(x => x.name === name);

        if (found) {
            return this;
        }

        const steps = [...this.steps, new WorkflowStepDto(name, color, [])];

        return this.with({ steps });
    }

    public changeStepColor(name: string, color: string) {
        return this.updateStep(name, x => x.changeColor(color));
    }

    public changeStepName(name: string, newName: string) {
        return this.updateStep(name, x => x.changeName(newName));
    }

    public removeStep(name: string) {
        const steps = this.steps.filter(x => x.name !== name).map(x => x.removeTransition(name));

        return this.with({ steps });
    }

    private updateStep(name: string, updater: (step: WorkflowStepDto) => WorkflowStepDto) {
        let found = this.steps.find(x => x.name === name);

        if (!found) {
            return this;
        }

        const newStep = updater(found);

        const steps = this.steps.map(x => x.name === name ? newStep : x.replaceTransition(newStep));

        return this.with({ steps });
    }
}

export class WorkflowStepDto extends Model<WorkflowStepDto> {
    constructor(
        public readonly name: string,
        public readonly color: string,
        public readonly transitions: WorkflowTransitionDto[]
    ) {
        super();
    }

    public changeName(name: string) {
        return this.with({ name });
    }

    public changeColor(color: string) {
        return this.with({ color });
    }

    public addTransition(step: WorkflowStepDto) {
        let found = this.transitions.find(x => x.step.name === step.name);

        if (found) {
            return this;
        }

        const transitions = [...this.transitions, new WorkflowTransitionDto(step) ];

        return this.with({ transitions });
    }

    public replaceTransition(step: WorkflowStepDto) {
        const transitions = this.transitions.map(x => x.step.name === step.name ? new WorkflowTransitionDto(step, x.expression) : x);

        return this.with({ transitions });
    }

    public removeTransition(name: string) {
        const transitions = this.transitions.filter(x => x.step.name !== name);

        return this.with({ transitions });
    }

    private updateStep(name: string, updater: (step: WorkflowStepDto) => WorkflowStepDto) {
        let found = this.steps.find(x => x.name === name);

        if (!found) {
            return this;
        }

        const newStep = updater(found);

        const steps = this.steps.map(x => x.name === name ? newStep : x.replaceTransition(newStep));

        return this.with({ steps });
    }
}

export class WorkflowTransitionDto extends Model<WorkflowTransitionDto> {
    constructor(
        public readonly step: WorkflowStepDto,
        public readonly expression?: string
    ) {
        super();
    }

    public updateExpression(expression?: string) {
        return this.with({ expression });
    }
}