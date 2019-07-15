/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';

import {
    Form,
    ImmutableArray,
    RuleDto,
    RuleElementDto,
    RulesState,
    SchemaDto
} from '@app/shared';

export const MODE_WIZARD = 'Wizard';
export const MODE_EDIT_TRIGGER = 'EditTrigger';
export const MODE_EDIT_ACTION  = 'EditAction';

@Component({
    selector: 'sqx-rule-wizard',
    styleUrls: ['./rule-wizard.component.scss'],
    templateUrl: './rule-wizard.component.html'
})
export class RuleWizardComponent implements AfterViewInit, OnInit {
    public actionForm = new Form<FormGroup, any>(new FormGroup({}));
    public actionType: string;
    public action: any = {};

    public triggerForm = new Form<FormGroup, any>(new FormGroup({}));
    public triggerType: string;
    public trigger: any = {};

    public isEditable: boolean;

    public step = 1;

    @Output()
    public complete = new EventEmitter();

    @Input()
    public ruleActions: { [name: string]: RuleElementDto };

    @Input()
    public ruleTriggers: { [name: string]: RuleElementDto };

    @Input()
    public schemas: ImmutableArray<SchemaDto>;

    @Input()
    public rule: RuleDto;

    @Input()
    public mode = MODE_WIZARD;

    constructor(
        private readonly rulesState: RulesState
    ) {
    }

    public ngOnInit() {
        this.isEditable = !this.rule || this.rule.canUpdate;

        if (this.mode === MODE_EDIT_ACTION) {
            this.step = 4;

            this.action = this.rule.action;
            this.actionType = this.rule.actionType;
        } else if (this.mode === MODE_EDIT_TRIGGER) {
            this.step = 2;

            this.trigger = this.rule.trigger;
            this.triggerType = this.rule.triggerType;
        }
    }

    public ngAfterViewInit() {
        this.actionForm.setEnabled(this.isEditable);

        this.triggerForm.setEnabled(this.isEditable);
    }

    public emitComplete() {
        this.complete.emit();
    }

    public selectTriggerType(type: string) {
        this.triggerType = type;
        this.step++;
    }

    public selectActionType(type: string) {
        this.actionType = type;
        this.step++;
    }

    public saveTrigger() {
        const value = this.triggerForm.submit();

        if (value) {
            this.trigger = { ...value, triggerType: this.triggerType };

            if (this.mode === MODE_WIZARD) {
                this.step++;
            } else {
                this.updateTrigger();
            }
        }
    }

    public saveAction() {
        const value = this.actionForm.submit();

        if (value) {
            this.action = { ...value, actionType: this.actionType };

            if (this.mode === MODE_WIZARD) {
                this.createRule();
            } else {
                this.updateAction();
            }
        }
    }

    private createRule() {
        const requestDto = { trigger: this.trigger, action: this.action };

        this.rulesState.create(requestDto)
            .subscribe(() => {
                this.emitComplete();

                this.actionForm.submitCompleted();
                this.triggerForm.submitCompleted();
            }, error => {
                this.actionForm.submitFailed(error);
                this.triggerForm.submitFailed(error);
            });
    }

    private updateTrigger() {
        if (!this.isEditable) {
            return;
        }

        this.rulesState.updateTrigger(this.rule, this.trigger)
            .subscribe(() => {
                this.emitComplete();

                this.triggerForm.submitCompleted();
            }, error => {
                this.triggerForm.submitFailed(error);
            });
    }

    private updateAction() {
        if (!this.isEditable) {
            return;
        }

        this.rulesState.updateAction(this.rule, this.action)
            .subscribe(() => {
                this.emitComplete();

                this.actionForm.submitCompleted();
            }, error => {
                this.actionForm.submitFailed(error);
            });
    }
}