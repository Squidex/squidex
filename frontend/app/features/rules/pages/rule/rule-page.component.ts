/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ActionForm, ALL_TRIGGERS, ResourceOwner, RuleDto, RuleElementDto, RulesService, RulesState, SchemasState, TriggerForm } from '@app/shared';

type ComponentState<T> = { type: string; values: any; form: T };

@Component({
    selector: 'sqx-rule-page',
    styleUrls: ['./rule-page.component.scss'],
    templateUrl: './rule-page.component.html',
})
export class RulePageComponent extends ResourceOwner implements OnInit {
    public supportedActions: { [name: string]: RuleElementDto };
    public supportedTriggers = ALL_TRIGGERS;

    public rule?: RuleDto | null;

    public currentAction?: ComponentState<ActionForm>;
    public currentTrigger?: ComponentState<TriggerForm>;

    public isEnabled = false;
    public isEditable = false;

    public get isManual() {
        return this.rule?.triggerType === 'Manual';
    }

    public get actionElement() {
        return this.supportedActions[this.currentAction?.type || ''];
    }

    public get triggerElement() {
        return this.supportedTriggers[this.currentTrigger?.type || ''];
    }

    constructor(
        public readonly rulesState: RulesState,
        public readonly rulesService: RulesService,
        public readonly schemasState: SchemasState,
        private readonly formBuilder: FormBuilder,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
    ) {
        super();
    }

    public ngOnInit() {
        this.rulesService.getActions()
            .subscribe(actions => {
                this.supportedActions = actions;

                this.initFromRule();
            });

        this.own(
            this.rulesState.selectedRule
                .subscribe(rule => {
                    this.rule = rule;

                    this.initFromRule();
                }));

        this.schemasState.loadIfNotLoaded();
    }

    private initFromRule() {
        if (this.rule && this.supportedActions) {
            this.isEditable = this.rule.canUpdate;
            this.isEnabled = this.rule.isEnabled;

            this.selectAction(this.rule.actionType, this.rule.action);
            this.selectTrigger(this.rule.triggerType, this.rule.trigger);
        } else {
            this.isEditable = true;
            this.isEnabled = false;

            this.resetAction();
            this.resetTrigger();
        }
    }

    public selectAction(type: string, values = {}) {
        const form = new ActionForm(this.supportedActions[type], type);

        form.setEnabled(this.isEditable);
        form.load(values);

        this.currentAction = { form, type, values };
    }

    public selectTrigger(type: string, values = {}) {
        const form = new TriggerForm(this.formBuilder, type);

        form.setEnabled(this.isEditable);
        form.load(values);

        this.currentTrigger = { form, type, values };
    }

    public resetAction() {
        this.currentAction = undefined;
    }

    public resetTrigger() {
        this.currentTrigger = undefined;
    }

    public trigger() {
        this.rulesState.trigger(this.rule!);
    }

    public save() {
        if (!this.isEditable || !this.currentAction || !this.currentTrigger) {
            return;
        }

        const action = this.currentAction.form.submit();

        if (!action) {
            return;
        }

        const trigger = this.currentTrigger.form.submit();

        if (!trigger || !action) {
            return;
        }

        const request: any = { trigger, action, isEnabled: this.isEnabled };

        if (this.rule) {
            this.rulesState.update(this.rule, request)
                .subscribe(() => {
                    this.submitCompleted();
                }, error => {
                    this.submitFailed(error);
                });
        } else {
            this.rulesState.create(request)
                .subscribe(rule => {
                    this.submitCompleted();

                    this.router.navigate([rule.id], { relativeTo: this.route.parent, replaceUrl: true });
                }, error => {
                    this.submitFailed(error);
                });
        }
    }

    private submitCompleted() {
        this.currentAction?.form.submitCompleted({ noReset: true });
        this.currentTrigger?.form.submitCompleted({ noReset: true });
    }

    private submitFailed(error: any) {
        this.currentAction?.form?.submitFailed(error);
        this.currentTrigger?.form?.submitFailed(error);
    }

    public back() {
        this.router.navigate(['../'], { relativeTo: this.route, replaceUrl: true });
    }
}
