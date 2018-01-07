/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';

import {
    AppContext,
    DateTime,
    fadeAnimation,
    ImmutableArray,
    ModalView,
    ruleActions,
    ruleTriggers,
    RuleDto,
    RulesService,
    SchemaDto,
    SchemasService
} from 'shared';

@Component({
    selector: 'sqx-rules-page',
    styleUrls: ['./rules-page.component.scss'],
    templateUrl: './rules-page.component.html',
    providers: [
        AppContext
    ],
    animations: [
        fadeAnimation
    ]
})
export class RulesPageComponent implements OnInit {
    public ruleActions = ruleActions;
    public ruleTriggers = ruleTriggers;

    public addRuleDialog = new ModalView();

    public rules: ImmutableArray<RuleDto>;
    public schemas: SchemaDto[];

    public wizardMode = 'Wizard';
    public wizardRule: RuleDto | null;

    constructor(public readonly ctx: AppContext,
        private readonly schemasService: SchemasService,
        private readonly rulesService: RulesService
    ) {
    }

    public ngOnInit() {
        this.load();
    }

    public load(showInfo = false) {
        this.schemasService.getSchemas(this.ctx.appName)
                .combineLatest(this.rulesService.getRules(this.ctx.appName), (s, w) => { return { rules: w, schemas: s }; })
            .subscribe(dtos => {
                this.schemas = dtos.schemas;
                this.rules = ImmutableArray.of(dtos.rules);

                if (showInfo) {
                    this.ctx.notifyInfo('Rules reloaded.');
                }
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public createNew() {
        this.wizardMode = 'Wizard';
        this.wizardRule = null;

        this.addRuleDialog.show();
    }

    public editTrigger(rule: RuleDto) {
        this.wizardMode = 'EditTrigger';
        this.wizardRule = rule;

        this.addRuleDialog.show();
    }

    public editAction(rule: RuleDto) {
        this.wizardMode = 'EditAction';
        this.wizardRule = rule;

        this.addRuleDialog.show();
    }

    public onRuleUpdated(rule: RuleDto) {
        this.rules = this.rules.replaceBy('id', rule);

        this.addRuleDialog.hide();
    }

    public onRuleCreated(rule: RuleDto) {
        this.rules = this.rules.push(rule);

        this.addRuleDialog.hide();
    }

    public toggleRule(rule: RuleDto) {
        if (rule.isEnabled) {
            this.rulesService.disableRule(this.ctx.appName, rule.id, rule.version)
                .subscribe(dto => {
                    this.rules = this.rules.replace(rule, rule.disable(this.ctx.userToken, dto.version, DateTime.now()));
                }, error => {
                    this.ctx.notifyError(error);
                });
        } else {
            this.rulesService.enableRule(this.ctx.appName, rule.id, rule.version)
                .subscribe(dto => {
                    this.rules = this.rules.replace(rule, rule.enable(this.ctx.userToken, dto.version, DateTime.now()));
                }, error => {
                    this.ctx.notifyError(error);
                });
        }
    }

    public deleteRule(rule: RuleDto) {
        this.rulesService.deleteRule(this.ctx.appName, rule.id, rule.version)
            .subscribe(dto => {
                this.rules = this.rules.remove(rule);
            }, error => {
                this.ctx.notifyError(error);
            });
    }
}
