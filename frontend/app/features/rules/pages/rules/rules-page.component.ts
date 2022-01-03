/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { ALL_TRIGGERS, RuleDto, RuleElementDto, RulesService, RulesState, SchemasState } from '@app/shared';

@Component({
    selector: 'sqx-rules-page',
    styleUrls: ['./rules-page.component.scss'],
    templateUrl: './rules-page.component.html',
})
export class RulesPageComponent implements OnInit {
    public supportedActions?: { [name: string]: RuleElementDto };
    public supportedTriggers = ALL_TRIGGERS;

    constructor(
        public readonly rulesState: RulesState,
        public readonly rulesService: RulesService,
        public readonly schemasState: SchemasState,
    ) {
    }

    public ngOnInit() {
        this.rulesState.load();

        this.rulesService.getActions()
            .subscribe(actions => {
                this.supportedActions = actions;
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
        this.rulesState.update(rule, { isEnabled: !rule.isEnabled });
    }

    public trackByRule(_index: number, rule: RuleDto) {
        return rule.id;
    }
}
