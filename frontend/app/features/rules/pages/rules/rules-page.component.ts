/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { ALL_TRIGGERS, DialogModel, RuleDto, RuleElementDto, RulesService, RulesState, SchemasState } from '@app/shared';

@Component({
    selector: 'sqx-rules-page',
    styleUrls: ['./rules-page.component.scss'],
    templateUrl: './rules-page.component.html'
})
export class RulesPageComponent implements OnInit {
    public addRuleDialog = new DialogModel();

    public wizardMode = 'Wizard';
    public wizardRule: RuleDto | null;

    public ruleActions: { [name: string]: RuleElementDto };
    public ruleTriggers = ALL_TRIGGERS;

    constructor(
        public readonly rulesState: RulesState,
        public readonly rulesService: RulesService,
        public readonly schemasState: SchemasState
    ) {
    }

    public ngOnInit() {
        this.rulesState.load();

        this.rulesService.getActions()
            .subscribe(actions => {
                this.ruleActions = actions;
            });

        this.schemasState.loadIfNotLoaded();
    }

    public reload() {
        this.rulesState.load(true);
    }

    public cancelRun() {
        this.rulesState.runCancel();
    }

    public delete(rule: RuleDto) {
        this.rulesState.delete(rule);
    }

    public toggle(rule: RuleDto) {
        if (rule.isEnabled) {
            this.rulesState.disable(rule);
        } else {
            this.rulesState.enable(rule);
        }
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

    public trackByRule(_index: number, rule: RuleDto) {
        return rule.id;
    }
}
