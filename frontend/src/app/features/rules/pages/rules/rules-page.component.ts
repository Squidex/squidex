/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ALL_TRIGGERS, DynamicRuleDto, DynamicUpdateRuleDto, LayoutComponent, ListViewComponent, RulesState, SchemasState, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourHintDirective, TourStepDirective, TranslatePipe } from '@app/shared';
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
    private readonly schemasState = inject(SchemasState);

    protected readonly rulesState = inject(RulesState);

    protected readonly availableTriggers = ALL_TRIGGERS;

    constructor() {
        this.rulesState.load();

        this.schemasState.loadIfNotLoaded();
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
