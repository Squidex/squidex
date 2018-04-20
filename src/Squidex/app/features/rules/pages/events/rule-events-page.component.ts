/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';

import {
    AppsState,
    DialogService,
    ImmutableArray,
    Pager,
    RuleEventDto,
    RulesService
} from '@app/shared';

@Component({
    selector: 'sqx-rule-events-page',
    styleUrls: ['./rule-events-page.component.scss'],
    templateUrl: './rule-events-page.component.html'
})
export class RuleEventsPageComponent implements OnInit {
    public eventsItems = ImmutableArray.empty<RuleEventDto>();
    public eventsPager = new Pager(0);

    public selectedEventId: string | null = null;

    constructor(
        public readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly rulesService: RulesService
    ) {
    }

    public ngOnInit() {
        this.load();
    }

    public load(notifyLoad = false) {
        this.rulesService.getEvents(this.appsState.appName, this.eventsPager.pageSize, this.eventsPager.skip)
            .subscribe(dtos => {
                this.eventsItems = ImmutableArray.of(dtos.items);
                this.eventsPager = this.eventsPager.setCount(dtos.total);

                if (notifyLoad) {
                    this.dialogs.notifyInfo('Events reloaded.');
                }
            }, error => {
                this.dialogs.notifyError(error);
            });
    }

    public enqueueEvent(event: RuleEventDto) {
        this.rulesService.enqueueEvent(this.appsState.appName, event.id)
            .subscribe(() => {
                this.dialogs.notifyInfo('Events enqueued. Will be resend in a few seconds.');
            }, error => {
                this.dialogs.notifyError(error);
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
}

