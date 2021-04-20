/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ALL_TRIGGERS, DialogModel, Form, ResourceOwner, RuleDto, RuleElementDto, RulesService, RulesState, SchemasState, TriggerType } from '@app/shared';

@Component({
    selector: 'sqx-rule-page',
    styleUrls: ['./rule-page.component.scss'],
    templateUrl: './rule-page.component.html'
})
export class RulePageComponent extends ResourceOwner implements OnInit {
    public addRuleDialog = new DialogModel();

    public supportedActions: { [name: string]: RuleElementDto };
    public supportedTriggers = ALL_TRIGGERS;

    public rule?: RuleDto | null;

    public formAction?: Form<FormGroup, any>;
    public formTrigger?: Form<FormGroup, any>;

    public actionProperties?: any;
    public actionType: string;

    public triggerProperties?: any;
    public triggerType: string;

    public name: string;

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
        private readonly route: ActivatedRoute,
        private readonly router: Router
    ) {
        super();
    }

    public ngOnInit() {
        this.rulesState.load();

        this.rulesService.getActions()
            .subscribe(actions => {
                this.supportedActions = actions;
            });

        this.own(
            this.rulesState.selectedRule
                .subscribe(rule => {
                    this.rule = rule;

                    if (rule) {
                        this.isEditable = rule.canUpdate;

                        this.name = rule.name;

                        this.selectAction(rule.action);
                        this.selectTrigger(rule.trigger);
                    } else {
                        this.isEditable = true;

                        this.name = '';

                        this.resetAction();
                        this.resetTrigger();
                    }

                    this.formTrigger?.setEnabled(this.isEditable);
                }));

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

        this.formAction = undefined;
    }

    public resetTrigger() {
        this.triggerProperties = undefined;
        this.triggerType = undefined!;

        this.formTrigger = undefined;
    }

    private selectAction(target: { actionType: string } & any) {
        const { actionType, ...properties } = target;

        this.actionProperties = properties;
        this.actionType = actionType;

        this.formAction = new Form<FormGroup, any>(new FormGroup({}));
        this.formAction.setEnabled(this.isEditable);
    }

    private selectTrigger(target: { triggerType: string } & any) {
        const { triggerType, ...properties } = target;

        this.triggerProperties = properties;
        this.triggerType = triggerType;

        this.formTrigger = new Form<FormGroup, any>(new FormGroup({}));
        this.formTrigger.setEnabled(this.isEditable);
    }

    public save() {
        if (!this.isEditable || !this.formAction || !this.formTrigger) {
            return;
        }

        const ruleTrigger = this.formTrigger.submit();
        const ruleAction = this.formAction.submit();

        if (!ruleTrigger || !ruleAction) {
            return;
        }

        const request = {
            trigger: {
                triggerType: this.triggerType,
                ...ruleTrigger
            },
            action: {
                actionType: this.actionType,
                ...ruleAction
            },
            name: this.name
        };

        if (this.rule) {
            this.rulesState.update(this.rule, request)
                .subscribe(() => {
                    this.formAction?.submitCompleted({ noReset: true });
                    this.formTrigger?.submitCompleted({ noReset: true });
                }, error => {
                    this.formAction?.submitFailed(error);
                    this.formTrigger?.submitFailed(error);
                });
        } else {
            this.rulesState.create(request)
                .subscribe(rule => {
                    this.formAction?.submitCompleted({ noReset: true });
                    this.formTrigger?.submitCompleted({ noReset: true });

                    this.router.navigate([rule.id], { relativeTo: this.route.parent, replaceUrl: true });
                }, error => {
                    this.formAction?.submitFailed(error);
                    this.formTrigger?.submitFailed(error);
                });
        }
    }

    public back() {
        this.router.navigate(['../'], { relativeTo: this.route, replaceUrl: true });
    }
}
