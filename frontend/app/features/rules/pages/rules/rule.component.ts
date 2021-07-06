/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { ActionsDto, fadeAnimation, ModalModel, RuleDto, RulesState, TriggersDto } from '@app/shared';

@Component({
    selector: 'sqx-rule',
    styleUrls: ['./rule.component.scss'],
    templateUrl: './rule.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RuleComponent {
    @Input()
    public ruleTriggers: TriggersDto;

    @Input()
    public ruleActions: ActionsDto;

    @Input()
    public rule: RuleDto;

    public dropdown = new ModalModel();

    public get isManual() {
        return this.rule.triggerType === 'Manual';
    }

    constructor(
        private readonly rulesState: RulesState,
    ) {
    }

    public delete() {
        this.rulesState.delete(this.rule);
    }

    public run() {
        this.rulesState.run(this.rule);
    }

    public runFromSnapshots() {
        this.rulesState.runFromSnapshots(this.rule);
    }

    public rename(name: string) {
        this.rulesState.update(this.rule, { name });
    }

    public toggle() {
        this.rulesState.update(this.rule, { isEnabled: !this.rule.isEnabled });
    }

    public trigger() {
        this.rulesState.trigger(this.rule);
    }
}
