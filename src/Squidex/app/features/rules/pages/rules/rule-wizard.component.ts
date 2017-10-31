/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, Output, ViewChild } from '@angular/core';

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
    SchemaDto
} from 'shared';

@Component({
    selector: 'sqx-rule-wizard',
    styleUrls: ['./rule-wizard.component.scss'],
    templateUrl: './rule-wizard.component.html',
    animations: [
        fadeAnimation
    ]
})
export class RuleWizardComponent extends AppComponentBase {
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

    @Input()
    public schemas: SchemaDto[];

    constructor(apps: AppsStoreService, dialogs: DialogService, authService: AuthService,
        private readonly rulesService: RulesService
    ) {
        super(dialogs, apps, authService);
    }

    public selectTriggerType(type: string) {
        this.triggerType = type;
        this.step++;
    }

    public selectTrigger(value: any) {
        this.trigger = Object.assign({}, value, { triggerType: this.triggerType });
        this.step++;
    }

    public selectActionType(type: string) {
        this.actionType = type;
        this.step++;
    }

    public selectAction(value: any) {
        this.action = Object.assign({}, value, { actionType: this.actionType });

        const requestDto = new CreateRuleDto(this.trigger, this.action);

        this.appNameOnce()
            .switchMap(app => this.rulesService.postRule(app, requestDto, this.authService.user!.id, DateTime.now()))
            .subscribe(dto => {
                this.created.emit(dto);
            }, error => {
                this.notifyError(error);
            });
    }

    public cancel() {
        this.cancelled.emit();
    }
}