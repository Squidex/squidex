/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { Router2State, RuleEventDto, RuleEventsState } from '@app/shared';

@Component({
    selector: 'sqx-rule-events-page',
    styleUrls: ['./rule-events-page.component.scss'],
    templateUrl: './rule-events-page.component.html',
    providers: [
        Router2State
    ]
})
export class RuleEventsPageComponent implements OnInit {
    public selectedEventId: string | null = null;

    constructor(
        public readonly ruleEventsRoute: Router2State,
        public readonly ruleEventsState: RuleEventsState
    ) {
    }

    public ngOnInit() {
        this.ruleEventsState.loadAndListen(this.ruleEventsRoute);
    }

    public reload() {
        this.ruleEventsState.load(true);
    }

    public enqueue(event: RuleEventDto) {
        this.ruleEventsState.enqueue(event);
    }

    public cancel(event: RuleEventDto) {
        this.ruleEventsState.cancel(event);
    }

    public selectEvent(id: string) {
        this.selectedEventId = this.selectedEventId !== id ? id : null;
    }

    public trackByRuleEvent(_index: number, ruleEvent: RuleEventDto) {
        return ruleEvent.id;
    }
}