/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, effect, signal, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { LayoutComponent, ListViewComponent, MessageBus, RuleElementDto, RuleSimulatorState, RulesService, SimulatedRuleEventDto, TitleComponent, TooltipDirective, TranslatePipe } from '@app/shared';
import { RuleConfigured } from '../messages';
import { SimulatedRuleEventComponent } from './simulated-rule-event.component';

@Component({
    selector: 'sqx-simulator-events-page',
    styleUrls: ['./rule-simulator-page.component.scss'],
    templateUrl: './rule-simulator-page.component.html',
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
    protected readonly ruleSimulatorState = inject(RuleSimulatorState);

    protected readonly availableSteps;

    protected readonly selectedRuleEvent = signal<string | null | undefined>(undefined);

    constructor() {
        const route = inject(ActivatedRoute);
        const rulesService = inject(RulesService);
        const messageBus = inject(MessageBus);

        this.availableSteps = toSignal(rulesService.getSteps(), { initialValue: {} as Record<string, RuleElementDto> });

        effect(onCleanup => {
            const subscriptions = [
                messageBus.of(RuleConfigured)
                    .subscribe(message => {
                        this.ruleSimulatorState.setRule(message.trigger, message.flow);
                    }),
                route.queryParams
                    .subscribe(query => {
                        this.ruleSimulatorState.selectRule(query['ruleId']);
                    }),
            ];

            onCleanup(() => {
                for (const subscription of subscriptions) {
                    subscription.unsubscribe();
                }
            });
        });
    }

    protected simulate() {
        this.ruleSimulatorState.load(true);
    }

    protected selectEvent(event: SimulatedRuleEventDto) {
        this.selectedRuleEvent.update(x => x === event.uniqueId ? null : event.uniqueId);
    }
}
