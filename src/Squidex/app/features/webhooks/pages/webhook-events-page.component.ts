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
    DialogService,
    ImmutableArray,
    Pager,
    WebhookEventDto,
    WebhooksService
} from 'shared';

@Component({
    selector: 'sqx-webhook-events-page',
    styleUrls: ['./webhook-events-page.component.scss'],
    templateUrl: './webhook-events-page.component.html'
})
export class WebhookEventsPageComponent extends AppComponentBase implements OnInit {
    public eventsItems = ImmutableArray.empty<WebhookEventDto>();
    public eventsPager = new Pager(0);

    public selectedEventId: string;

    constructor(dialogs: DialogService, appsStore: AppsStoreService,
        private readonly webhooksService: WebhooksService
    ) {
        super(dialogs, appsStore);
    }

    public ngOnInit() {
        this.load();
    }

    public load(showInfo = false) {
        this.appNameOnce()
            .switchMap(app => this.webhooksService.getEvents(app, this.eventsPager.pageSize, this.eventsPager.skip))
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

    public enqueueEvent(event: WebhookEventDto) {
        this.appNameOnce()
            .switchMap(app => this.webhooksService.enqueueEvent(app, event.id))
            .subscribe(dtos => {
                this.notifyInfo('Events enqueued. Will be send in a few seconds.');
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

