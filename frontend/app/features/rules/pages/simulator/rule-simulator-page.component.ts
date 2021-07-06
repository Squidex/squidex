/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ResourceOwner, RuleSimulatorState, SimulatedRuleEventDto } from '@app/shared';

@Component({
    selector: 'sqx-simulator-events-page',
    styleUrls: ['./rule-simulator-page.component.scss'],
    templateUrl: './rule-simulator-page.component.html',
})
export class RuleSimulatorPageComponent extends ResourceOwner implements OnInit {
    public selectedRuleEvent?: SimulatedRuleEventDto | null;

    constructor(
        private route: ActivatedRoute,
        public readonly ruleSimulatorState: RuleSimulatorState,
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.route.queryParams
                .subscribe(query => {
                    this.ruleSimulatorState.selectRule(query['ruleId']);
                }));
    }

    public simulate() {
        this.ruleSimulatorState.load();
    }

    public selectEvent(event: SimulatedRuleEventDto) {
        if (this.selectedRuleEvent === event) {
            this.selectedRuleEvent = null;
        } else {
            this.selectedRuleEvent = event;
        }
    }
}
