/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';

import {
    AppComponentBase,
    AppsStoreService,
    AuthService,
    DateTime,
    DialogService,
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
    animations: [
        fadeAnimation
    ]
})
export class RulesPageComponent extends AppComponentBase implements OnInit {
    public ruleActions = ruleActions;
    public ruleTriggers = ruleTriggers;

    public addRuleDialog = new ModalView();

    public rules: ImmutableArray<RuleDto>;
    public schemas: SchemaDto[];

    public wizardMode = 'Wizard';
    public wizardRule: RuleDto;

    constructor(apps: AppsStoreService, dialogs: DialogService, authService: AuthService,
        private readonly schemasService: SchemasService,
        private readonly rulesService: RulesService
    ) {
        super(dialogs, apps, authService);
    }

    public ngOnInit() {
        this.load();
    }

    public load(showInfo = false) {
        this.appNameOnce()
            .switchMap(app =>
                this.schemasService.getSchemas(app)
                    .combineLatest(this.rulesService.getRules(app),
                        (s, w) => { return { rules: w, schemas: s }; }))
            .subscribe(dtos => {
                this.schemas = dtos.schemas;
                this.rules = ImmutableArray.of(dtos.rules);

                if (showInfo) {
                    this.notifyInfo('Rules reloaded.');
                }
            }, error => {
                this.notifyError(error);
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

    public onRuleCreated(rule: RuleDto) {
        this.rules = this.rules.push(rule);

        this.addRuleDialog.hide();
    }

    public toggleRule(rule: RuleDto) {
        if (rule.isEnabled) {
            this.appNameOnce()
                .switchMap(app => this.rulesService.disableRule(app, rule.id, rule.version))
                .subscribe(dto => {
                    this.rules = this.rules.replace(rule, rule.disable(this.authService.user.id, dto.version, DateTime.now()));
                }, error => {
                    this.notifyError(error);
                });
        } else {
            this.appNameOnce()
                .switchMap(app => this.rulesService.enableRule(app, rule.id, rule.version))
                .subscribe(dto => {
                    this.rules = this.rules.replace(rule, rule.enable(this.authService.user.id, dto.version, DateTime.now()));
                }, error => {
                    this.notifyError(error);
                });
        }
    }

    public deleteRule(rule: RuleDto) {
        this.appNameOnce()
            .switchMap(app => this.rulesService.deleteRule(app, rule.id, rule.version))
            .subscribe(dto => {
                this.rules = this.rules.remove(rule);
            }, error => {
                this.notifyError(error);
            });
    }
}
