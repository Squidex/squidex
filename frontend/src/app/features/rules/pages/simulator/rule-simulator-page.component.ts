/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MessageBus, ResourceOwner, RuleSimulatorState, SimulatedRuleEventDto } from '@app/shared';
import { RuleConfigured } from '../messages';

@Component({
    selector: 'sqx-simulator-events-page',
    styleUrls: ['./rule-simulator-page.component.scss'],
    templateUrl: './rule-simulator-page.component.html',
})
export class RuleSimulatorPageComponent extends ResourceOwner implements OnInit {
    public selectedRuleEvent?: string | null;

    constructor(
        public readonly ruleSimulatorState: RuleSimulatorState,
        private readonly route: ActivatedRoute,
        private readonly messageBus: MessageBus,
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.messageBus.of(RuleConfigured)
                .subscribe(message => {
                    this.ruleSimulatorState.setRule(message.trigger, message.action);
                }));

        this.own(
            this.route.queryParams
                .subscribe(query => {
                    this.ruleSimulatorState.selectRule(query['ruleId']);
                }));
    }

    public simulate() {
        this.ruleSimulatorState.load(true);
    }

    public selectEvent(event: SimulatedRuleEventDto) {
        if (this.selectedRuleEvent === event.eventId) {
            this.selectedRuleEvent = null;
        } else {
            this.selectedRuleEvent = event.eventId;
        }
    }

    public trackByEvent(_index: number, event: SimulatedRuleEventDto) {
        return event.eventId;
    }
}
