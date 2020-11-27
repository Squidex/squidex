/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: component-selector

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { ActionsDto, fadeAnimation, ModalModel, RuleDto, RulesState, TriggersDto } from '@app/shared';

@Component({
    selector: 'sqx-rule',
    styleUrls: ['./rule.component.scss'],
    templateUrl: './rule.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RuleComponent {
    @Output()
    public editTrigger = new EventEmitter();

    @Output()
    public editAction = new EventEmitter();

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
        private readonly rulesState: RulesState
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
        this.rulesState.rename(this.rule, name);
    }

    public trigger() {
        this.rulesState.trigger(this.rule);
    }

    public emitEditAction() {
        this.editAction.emit();
    }

    public emitEditTrigger() {
        if (!this.isManual) {
            this.editTrigger.emit();
        }
    }

    public toggle() {
        if (this.rule.isEnabled) {
            this.rulesState.disable(this.rule);
        } else {
            this.rulesState.enable(this.rule);
        }
    }
}