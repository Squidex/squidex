/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { LayoutComponent, ListViewComponent, MessageBus, RuleSimulatorState, RulesState, SimulatedRuleEventDto, TitleComponent, TooltipDirective, TranslatePipe } from '@app/shared';
import { RuleConfigured } from '../messages';
import { SimulatedRuleEventComponent } from './simulated-rule-event.component';

@Component({
    selector: 'sqx-simulator-events-page',
    styleUrls: ['./rule-simulator-page.component.scss'],
    templateUrl: './rule-simulator-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        LayoutComponent,
        ListViewComponent,
        SimulatedRuleEventComponent,
        TitleComponent,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class RuleSimulatorPageComponent {
    private readonly messageBus = inject(MessageBus);
    private readonly route = inject(ActivatedRoute);

    protected readonly rulesState = inject(RulesState);

    protected readonly ruleSimulatorState = inject(RuleSimulatorState);

    protected readonly selectedRuleEvent = signal<string | null | undefined>(undefined);

    constructor() {
        this.messageBus.on(RuleConfigured, message => {
            this.ruleSimulatorState.setRule(message.trigger, message.flow);
        });

        this.route.queryParams
            .pipe(takeUntilDestroyed())
            .subscribe(query => {
                this.ruleSimulatorState.selectRule(query['ruleId']);
            });
    }

    protected simulate() {
        this.ruleSimulatorState.load(true);
    }

    protected selectEvent(event: SimulatedRuleEventDto) {
        this.selectedRuleEvent.update(x => x === event.uniqueId ? null : event.uniqueId);
    }
}
