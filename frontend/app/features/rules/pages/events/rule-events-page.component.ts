/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import {
    ResourceOwner,
    RuleEventDto,
    RuleEventsState
} from '@app/shared';

@Component({
    selector: 'sqx-rule-events-page',
    styleUrls: ['./rule-events-page.component.scss'],
    templateUrl: './rule-events-page.component.html'
})
export class RuleEventsPageComponent extends ResourceOwner implements OnInit {
    public selectedEventId: string | null = null;

    constructor(
        public readonly ruleEventsState: RuleEventsState,
        private readonly route: ActivatedRoute
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.route.queryParams
                .subscribe(x => {
                    this.ruleEventsState.filterByRule(x.ruleId);
                }));

        this.ruleEventsState.load();
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

    public trackByRuleEvent(index: number, ruleEvent: RuleEventDto) {
        return ruleEvent.id;
    }
}