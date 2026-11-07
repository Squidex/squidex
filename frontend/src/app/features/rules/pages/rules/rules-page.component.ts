/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ALL_TRIGGERS, DynamicRuleDto, DynamicUpdateRuleDto, LayoutComponent, ListViewComponent, RulesService, RulesState, SchemasState, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourHintDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { RuleComponent } from './rule.component';

@Component({
    selector: 'sqx-rules-page',
    styleUrls: ['./rules-page.component.scss'],
    templateUrl: './rules-page.component.html',
    imports: [
        AsyncPipe,
        LayoutComponent,
        ListViewComponent,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        RuleComponent,
        ShortcutDirective,
        SidebarMenuDirective,
        TitleComponent,
        TooltipDirective,
        TourHintDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class RulesPageComponent {
    public readonly availableTriggers = ALL_TRIGGERS;
    public readonly availableSteps;

    constructor(
        public readonly rulesState: RulesState,
        public readonly rulesService: RulesService,
        public readonly schemasState: SchemasState,
    ) {
        this.availableSteps = toSignal(rulesService.getSteps());

        rulesState.load();

        schemasState.loadIfNotLoaded();
    }

    public reload() {
        this.rulesState.load(true);
    }

    public cancelRun() {
        this.rulesState.runCancel();
    }

    public delete(rule: DynamicRuleDto) {
        this.rulesState.delete(rule);
    }

    public toggle(rule: DynamicRuleDto) {
        this.rulesState.update(rule, new DynamicUpdateRuleDto({ isEnabled: !rule.isEnabled }));
    }
}
