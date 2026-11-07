/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
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
    protected readonly availableSteps = toSignal(inject(RulesService).getSteps(), { initialValue: {} as Record<string, RuleElementDto> });
    protected readonly selectedRuleEvent = signal<string | null | undefined>(undefined);

    constructor() {
        inject(MessageBus).on(RuleConfigured, message => {
            this.ruleSimulatorState.setRule(message.trigger, message.flow);
        });

        inject(ActivatedRoute).queryParams
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
