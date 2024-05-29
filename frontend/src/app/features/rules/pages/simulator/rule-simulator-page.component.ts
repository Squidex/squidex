/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LayoutComponent, ListViewComponent, MessageBus, RuleSimulatorState, SimulatedRuleEventDto, Subscriptions, TitleComponent, TooltipDirective, TranslatePipe } from '@app/shared';
import { RuleConfigured } from '../messages';
import { SimulatedRuleEventComponent } from './simulated-rule-event.component';

@Component({
    standalone: true,
    selector: 'sqx-simulator-events-page',
    styleUrls: ['./rule-simulator-page.component.scss'],
    templateUrl: './rule-simulator-page.component.html',
    imports: [
        AsyncPipe,
        LayoutComponent,
        ListViewComponent,
        SimulatedRuleEventComponent,
        TitleComponent,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class RuleSimulatorPageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public selectedRuleEvent?: string | null;

    constructor(
        public readonly ruleSimulatorState: RuleSimulatorState,
        private readonly route: ActivatedRoute,
        private readonly messageBus: MessageBus,
    ) {
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.messageBus.of(RuleConfigured)
                .subscribe(message => {
                    this.ruleSimulatorState.setRule(message.trigger, message.action);
                }));

        this.subscriptions.add(
            this.route.queryParams
                .subscribe(query => {
                    this.ruleSimulatorState.selectRule(query['ruleId']);
                }));
    }

    public simulate() {
        this.ruleSimulatorState.load(true);
    }

    public selectEvent(event: SimulatedRuleEventDto) {
        if (this.selectedRuleEvent === event.uniqueId) {
            this.selectedRuleEvent = null;
        } else {
            this.selectedRuleEvent = event.uniqueId;
        }
    }
}
