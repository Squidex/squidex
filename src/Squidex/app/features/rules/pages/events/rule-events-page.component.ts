/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { onErrorResumeNext } from 'rxjs/operators';

import {
    AppsState,
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
        public readonly appsState: AppsState,
        public readonly ruleEventsState: RuleEventsState
    ) {
    }

    public ngOnInit() {
        this.ruleEventsState.load().pipe(onErrorResumeNext()).subscribe();
    }

    public reload() {
        this.ruleEventsState.load(true).pipe(onErrorResumeNext()).subscribe();
    }

    public goNext() {
        this.ruleEventsState.goNext().pipe(onErrorResumeNext()).subscribe();
    }

    public goPrev() {
        this.ruleEventsState.goPrev().pipe(onErrorResumeNext()).subscribe();
    }

    public enqueue(event: RuleEventDto) {
        this.ruleEventsState.enqueue(event).pipe(onErrorResumeNext()).subscribe();
    }

    public selectEvent(id: string) {
        this.selectedEventId = this.selectedEventId !== id ? id : null;
    }

    public trackByRuleEvent(index: number, ruleEvent: RuleEventDto) {
        return ruleEvent.id;
    }
}

