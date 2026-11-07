/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

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
    protected readonly availableTriggers = ALL_TRIGGERS;
    protected readonly availableSteps;

    constructor(
        protected readonly rulesState: RulesState,
        rulesService: RulesService,
        schemasState: SchemasState,
    ) {
        this.availableSteps = toSignal(rulesService.getSteps());

        rulesState.load();

        schemasState.loadIfNotLoaded();
    }

    protected reload() {
        this.rulesState.load(true);
    }

    protected cancelRun() {
        this.rulesState.runCancel();
    }

    protected delete(rule: DynamicRuleDto) {
        this.rulesState.delete(rule);
    }

    protected toggle(rule: DynamicRuleDto) {
        this.rulesState.update(rule, new DynamicUpdateRuleDto({ isEnabled: !rule.isEnabled }));
    }
}
