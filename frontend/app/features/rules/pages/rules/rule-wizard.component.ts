/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { Form, RuleDto, RuleElementDto, RulesState, SchemaDto, TriggerType } from '@app/shared';

const MODE_WIZARD = 'Wizard';
const MODE_EDIT_TRIGGER = 'EditTrigger';
const MODE_EDIT_ACTION  = 'EditAction';

@Component({
    selector: 'sqx-rule-wizard',
    styleUrls: ['./rule-wizard.component.scss'],
    templateUrl: './rule-wizard.component.html'
})
export class RuleWizardComponent implements AfterViewInit, OnInit {
    @Output()
    public complete = new EventEmitter();

    @Input()
    public ruleActions: { [name: string]: RuleElementDto };

    @Input()
    public ruleTriggers: { [name: string]: RuleElementDto };

    @Input()
    public schemas: ReadonlyArray<SchemaDto>;

    @Input()
    public rule: RuleDto;

    @Input()
    public mode = MODE_WIZARD;

    public actionForm = new Form<FormGroup, any>(new FormGroup({}));
    public action: any = {};

    public triggerForm = new Form<FormGroup, any>(new FormGroup({}));
    public trigger: any = {};

    public get isWizard() {
        return this.mode === MODE_WIZARD;
    }

    public get actionElement() {
        return this.ruleActions[this.action.actionType];
    }

    public get triggerElement() {
        return this.ruleTriggers[this.trigger.triggerType];
    }

    public isEditable: boolean;

    public step = 1;

    constructor(
        private readonly rulesState: RulesState
    ) {
    }

    public ngOnInit() {
        this.isEditable = !this.rule || this.rule.canUpdate;

        if (this.mode === MODE_EDIT_ACTION) {
            this.step = 4;

            this.action = this.rule.action;
        } else if (this.mode === MODE_EDIT_TRIGGER) {
            this.step = 2;

            this.trigger = this.rule.trigger;
        }
    }

    public ngAfterViewInit() {
        this.actionForm.setEnabled(this.isEditable);

        this.triggerForm.setEnabled(this.isEditable);
    }

    public emitComplete() {
        this.complete.emit();
    }

    public go(step: number) {
        this.step = step;
    }

    public selectActionType(type: string) {
        this.action = { actionType: type };
        this.actionForm = new Form<FormGroup, any>(new FormGroup({}));

        this.step++;
    }

    public selectTriggerType(type: TriggerType) {
        this.trigger = { triggerType: type };
        this.triggerForm = new Form<FormGroup, any>(new FormGroup({}));

        if (type === 'Manual') {
            this.step += 2;
        } else {
            this.step++;
        }
    }

    public saveTrigger() {
        const value = this.triggerForm.submit();

        if (value) {
            this.trigger = { ...value, triggerType: this.trigger.triggerType };

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
            this.action = { ...value, actionType: this.action.actionType };

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

                this.triggerForm.submitCompleted({ noReset: true });
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

                this.actionForm.submitCompleted({ noReset: true });
            }, error => {
                this.actionForm.submitFailed(error);
            });
    }
}