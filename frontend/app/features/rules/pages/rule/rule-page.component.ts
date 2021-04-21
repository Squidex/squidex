/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ActionForm, ALL_TRIGGERS, ResourceOwner, RuleDto, RuleElementDto, RulesService, RulesState, SchemasState, TriggerForm, TriggerType } from '@app/shared';

@Component({
    selector: 'sqx-rule-page',
    styleUrls: ['./rule-page.component.scss'],
    templateUrl: './rule-page.component.html'
})
export class RulePageComponent extends ResourceOwner implements OnInit {
    public supportedActions: { [name: string]: RuleElementDto };
    public supportedTriggers = ALL_TRIGGERS;

    public rule?: RuleDto | null;

    public formForAction?: ActionForm;
    public formForTrigger?: TriggerForm;

    public actionProperties?: any;
    public actionType: string;

    public triggerProperties?: any;
    public triggerType: string;

    public isEnabled = false;

    public get actionElement() {
        return this.supportedActions[this.actionType];
    }

    public get triggerElement() {
        return this.supportedTriggers[this.triggerType];
    }

    public isEditable = false;

    constructor(
        public readonly rulesState: RulesState,
        public readonly rulesService: RulesService,
        public readonly schemasState: SchemasState,
        private readonly formBuilder: FormBuilder,
        private readonly route: ActivatedRoute,
        private readonly router: Router
    ) {
        super();
    }

    public ngOnInit() {
        this.rulesService.getActions()
            .subscribe(actions => {
                this.supportedActions = actions;

                this.own(
                    this.rulesState.selectedRule
                        .subscribe(rule => {
                            this.rule = rule;

                            if (rule) {
                                this.isEditable = rule.canUpdate;
                                this.isEnabled = rule.isEnabled;

                                this.selectAction(rule.action);
                                this.selectTrigger(rule.trigger);
                            } else {
                                this.isEditable = true;
                                this.isEnabled = false;

                                this.resetAction();
                                this.resetTrigger();
                            }

                            this.formForTrigger?.setEnabled(this.isEditable);
                        }));
            });

        this.schemasState.loadIfNotLoaded();
    }

    public selectActionType(actionType: string) {
        this.selectAction({ actionType });
    }

    public selectTriggerType(triggerType: TriggerType) {
        this.selectTrigger({ triggerType });
    }

    public resetAction() {
        this.actionProperties = undefined;
        this.actionType = undefined!;

        this.formForAction = undefined;
    }

    public resetTrigger() {
        this.triggerProperties = undefined;
        this.triggerType = undefined!;

        this.formForTrigger = undefined;
    }

    private selectAction(target: { actionType: string } & any) {
        const { actionType, ...properties } = target;

        if (!this.formForAction || actionType !== this.actionType) {
            this.formForAction = new ActionForm(this.supportedActions[actionType], actionType);
        }

        this.actionProperties = properties;
        this.actionType = actionType;

        this.formForAction?.setEnabled(this.isEditable);
        this.formForAction?.load(properties);
    }

    private selectTrigger(target: { triggerType: string } & any) {
        const { triggerType, ...properties } = target;

        if (!this.formForTrigger || triggerType !== this.triggerType) {
            this.formForTrigger = new TriggerForm(this.formBuilder, triggerType);
        }

        this.triggerProperties = properties;
        this.triggerType = triggerType;

        this.formForTrigger.setEnabled(this.isEditable);
        this.formForTrigger.load(properties);
    }

    public save() {
        if (!this.isEditable || !this.formForAction || !this.formForTrigger) {
            return;
        }

        const trigger = this.formForTrigger.submit();
        const action = this.formForAction.submit();

        if (!trigger || !action) {
            return;
        }

        const request = { trigger, action, isEnabled: this.isEnabled };

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
        this.formForAction?.submitCompleted({ noReset: true });
        this.formForTrigger?.submitCompleted({ noReset: true });
    }

    private submitFailed(error: any) {
        this.formForAction?.submitFailed(error);
        this.formForTrigger?.submitFailed(error);
    }

    public back() {
        this.router.navigate(['../'], { relativeTo: this.route, replaceUrl: true });
    }
}
