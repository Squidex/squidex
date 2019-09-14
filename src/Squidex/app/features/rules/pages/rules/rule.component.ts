/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: component-selector

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';

import {
    ActionsDto,
    RuleDto,
    RulesState,
    TriggersDto
} from '@app/shared';

@Component({
    selector: '[sqxRule]',
    template: `
        <tr>
            <td class="cell-separator">
                <h3>If</h3>
            </td>
            <td class="cell-auto">
                <span (click)="editTrigger.emit()">
                    <sqx-rule-element [type]="rule.triggerType" [element]="ruleTriggers[rule.triggerType]"></sqx-rule-element>
                </span>
            </td>
            <td class="cell-separator">
                <h3>then</h3>
            </td>
            <td class="cell-auto">
                <span (click)="editAction.emit()">
                    <sqx-rule-element [type]="rule.actionType" [element]="ruleActions[rule.actionType]"></sqx-rule-element>
                </span>
            </td>
            <td class="cell-actions">
                <sqx-toggle [disabled]="!rule.canDisable && !rule.canEnable" [ngModel]="rule.isEnabled" (ngModelChange)="toggle()"></sqx-toggle>
            </td>
            <td class="cell-actions">
                <button type="button" class="btn btn-text-danger"
                    [disabled]="!rule.canDelete"
                    (sqxConfirmClick)="delete()"
                    confirmTitle="Delete rule"
                    confirmText="Do you really want to delete the rule?">
                    <i class="icon-bin2"></i>
                </button>
            </td>
        </tr>
        <tr class="spacer"></tr>
    `,
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

    @Input('sqxRule')
    public rule: RuleDto;

    constructor(
        private readonly rulesState: RulesState
    ) {
    }

    public delete() {
        this.rulesState.delete(this.rule);
    }

    public toggle() {
        if (this.rule.isEnabled) {
            this.rulesState.disable(this.rule);
        } else {
            this.rulesState.enable(this.rule);
        }
    }
}