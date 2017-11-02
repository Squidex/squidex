/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';

import {
    AppComponentBase,
    AppsStoreService,
    AuthService,
    CreateRuleDto,
    DateTime,
    DialogService,
    fadeAnimation,
    ruleActions,
    ruleTriggers,
    RuleDto,
    RulesService,
    SchemaDto,
    UpdateRuleDto
} from 'shared';

export const MODE_WIZARD = 'Wizard';
export const MODE_EDIT_TRIGGER = 'EditTrigger';
export const MODE_EDIT_ACTION  = 'EditAction';

@Component({
    selector: 'sqx-rule-wizard',
    styleUrls: ['./rule-wizard.component.scss'],
    templateUrl: './rule-wizard.component.html',
    animations: [
        fadeAnimation
    ]
})
export class RuleWizardComponent extends AppComponentBase implements OnInit {
    public ruleActions = ruleActions;
    public ruleTriggers = ruleTriggers;

    public triggerType: string;
    public trigger: any = {};
    public actionType: string;
    public action: any = {};
    public step = 1;

    @ViewChild('triggerControl')
    public triggerControl: any;

    @ViewChild('actionControl')
    public actionControl: any;

    @Output()
    public cancelled = new EventEmitter();

    @Output()
    public created = new EventEmitter<RuleDto>();

    @Output()
    public updated = new EventEmitter<RuleDto>();

    @Input()
    public schemas: SchemaDto[];

    @Input()
    public rule: RuleDto;

    @Input()
    public mode = MODE_WIZARD;

    constructor(apps: AppsStoreService, dialogs: DialogService, authService: AuthService,
        private readonly rulesService: RulesService
    ) {
        super(dialogs, apps, authService);
    }

    public ngOnInit() {
        if (this.mode === MODE_EDIT_ACTION) {
            this.step = 4;

            this.action = Object.assign({}, this.rule.action);
            this.actionType = this.rule.actionType;

            delete this.action.actionType;
        } else if (this.mode === MODE_EDIT_TRIGGER) {
            this.step = 2;

            this.trigger = Object.assign({}, this.rule.trigger);
            this.triggerType = this.rule.triggerType;

            delete this.trigger.triggerType;
        }
    }

    public selectTriggerType(type: string) {
        this.triggerType = type;
        this.step++;
    }

    public selectActionType(type: string) {
        this.actionType = type;
        this.step++;
    }

    public selectTrigger(value: any) {
        this.trigger = Object.assign({}, value, { triggerType: this.triggerType });

        if (this.mode === MODE_WIZARD) {
            this.step++;
        } else {
            this.updateTrigger();
        }
    }

    public selectAction(value: any) {
        this.action = Object.assign({}, value, { actionType: this.actionType });

        if (this.mode === MODE_WIZARD) {
            this.createRule();
        } else {
            this.updateAction();
        }
    }

    private createRule() {
        const requestDto = new CreateRuleDto(this.trigger, this.action);

        this.appNameOnce()
            .switchMap(app => this.rulesService.postRule(app, requestDto, this.authService.user!.id, DateTime.now()))
            .subscribe(dto => {
                this.created.emit(dto);
            }, error => {
                this.notifyError(error);
            });
    }

    private updateTrigger() {
        const requestDto = new UpdateRuleDto(this.trigger, null);

        this.appNameOnce()
            .switchMap(app => this.rulesService.putRule(app, this.rule.id, requestDto, this.rule.version))
            .subscribe(dto => {
                const rule = this.rule.updateTrigger(this.trigger, this.authService.user.id, dto.version, DateTime.now());
                this.updated.emit(rule);
            }, error => {
                this.notifyError(error);
            });
    }

    private updateAction() {
        const requestDto = new UpdateRuleDto(null, this.action);

        this.appNameOnce()
            .switchMap(app => this.rulesService.putRule(app, this.rule.id, requestDto, this.rule.version))
            .subscribe(dto => {
                const rule = this.rule.updateAction(this.action, this.authService.user.id, dto.version, DateTime.now());

                this.updated.emit(rule);
            }, error => {
                this.notifyError(error);
            });
    }

    public cancel() {
        this.cancelled.emit();
    }
}