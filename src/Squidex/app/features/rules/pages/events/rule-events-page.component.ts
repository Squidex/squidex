/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';

import {
    AppComponentBase,
    AppsStoreService,
    AuthService,
    DialogService,
    ImmutableArray,
    Pager,
    RuleEventDto,
    RulesService
} from 'shared';

@Component({
    selector: 'sqx-rule-events-page',
    styleUrls: ['./rule-events-page.component.scss'],
    templateUrl: './rule-events-page.component.html'
})
export class RuleEventsPageComponent extends AppComponentBase implements OnInit {
    public eventsItems = ImmutableArray.empty<RuleEventDto>();
    public eventsPager = new Pager(0);

    public selectedEventId: string | null = null;

    constructor(dialogs: DialogService, appsStore: AppsStoreService, authService: AuthService,
        private readonly rulesService: RulesService
    ) {
        super(dialogs, appsStore, authService);
    }

    public ngOnInit() {
        this.load();
    }

    public load(showInfo = false) {
        this.appNameOnce()
            .switchMap(app => this.rulesService.getEvents(app, this.eventsPager.pageSize, this.eventsPager.skip))
            .subscribe(dtos => {
                this.eventsItems = ImmutableArray.of(dtos.items);
                this.eventsPager = this.eventsPager.setCount(dtos.total);

                if (showInfo) {
                    this.notifyInfo('Events reloaded.');
                }
            }, error => {
                this.notifyError(error);
            });
    }

    public enqueueEvent(event: RuleEventDto) {
        this.appNameOnce()
            .switchMap(app => this.rulesService.enqueueEvent(app, event.id))
            .subscribe(() => {
                this.notifyInfo('Events enqueued. Will be resend in a few seconds.');
            }, error => {
                this.notifyError(error);
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
            return 'default';
        } else {
            return status.toLowerCase();
        }
    }
}

