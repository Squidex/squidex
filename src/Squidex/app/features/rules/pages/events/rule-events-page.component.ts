/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';

import {
    AppContext,
    ImmutableArray,
    Pager,
    RuleEventDto,
    RulesService
} from 'shared';

@Component({
    selector: 'sqx-rule-events-page',
    styleUrls: ['./rule-events-page.component.scss'],
    templateUrl: './rule-events-page.component.html',
    providers: [
        AppContext
    ]
})
export class RuleEventsPageComponent implements OnInit {
    public eventsItems = ImmutableArray.empty<RuleEventDto>();
    public eventsPager = new Pager(0);

    public selectedEventId: string | null = null;

    constructor(public readonly ctx: AppContext,
        private readonly rulesService: RulesService
    ) {
    }

    public ngOnInit() {
        this.load();
    }

    public load(showInfo = false) {
        this.rulesService.getEvents(this.ctx.appName, this.eventsPager.pageSize, this.eventsPager.skip)
            .subscribe(dtos => {
                this.eventsItems = ImmutableArray.of(dtos.items);
                this.eventsPager = this.eventsPager.setCount(dtos.total);

                if (showInfo) {
                    this.ctx.notifyInfo('Events reloaded.');
                }
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public enqueueEvent(event: RuleEventDto) {
        this.rulesService.enqueueEvent(this.ctx.appName, event.id)
            .subscribe(() => {
                this.ctx.notifyInfo('Events enqueued. Will be resend in a few seconds.');
            }, error => {
                this.ctx.notifyError(error);
            });
    }

    public selectEvent(id: string) {
        this.selectedEventId = this.selectedEventId !== id ? id : null;
    }

    public goNext() {
        this.eventsPager = this.eventsPager.goNext();

        this.load();
    }

    public goPrev() {
        this.eventsPager = this.eventsPager.goPrev();

        this.load();
    }

    public getBadgeClass(status: string) {
        if (status === 'Retry') {
            return 'warning';
        } else if (status === 'Failed') {
            return 'danger';
        } else if (status === 'Pending') {
            return 'secondary';
        } else {
            return status.toLowerCase();
        }
    }
}

