/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';

import {
    RuleEventDto,
    RuleEventsState
} from '@app/shared';

@Component({
    selector: 'sqx-rule-events-page',
    styleUrls: ['./rule-events-page.component.scss'],
    templateUrl: './rule-events-page.component.html'
})
export class RuleEventsPageComponent implements OnInit {
    public selectedEventId: string | null = null;

    constructor(
        public readonly ruleEventsState: RuleEventsState
    ) {
    }

    public ngOnInit() {
        this.ruleEventsState.load();
    }

    public reload() {
        this.ruleEventsState.load(true);
    }

    public goNext() {
        this.ruleEventsState.goNext();
    }

    public goPrev() {
        this.ruleEventsState.goPrev();
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