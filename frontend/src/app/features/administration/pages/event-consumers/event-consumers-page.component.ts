/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { DialogModel, LayoutComponent, ListViewComponent, ModalDialogComponent, ModalDirective, ShortcutDirective, SidebarMenuDirective, Subscriptions, SyncWidthDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { EventConsumerDto, EventConsumersState } from '../../internal';
import { EventConsumerComponent } from './event-consumer.component';

@Component({
    standalone: true,
    selector: 'sqx-event-consumers-page',
    styleUrls: ['./event-consumers-page.component.scss'],
    templateUrl: './event-consumers-page.component.html',
    imports: [
        AsyncPipe,
        EventConsumerComponent,
        LayoutComponent,
        ListViewComponent,
        ModalDialogComponent,
        ModalDirective,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        ShortcutDirective,
        SidebarMenuDirective,
        SyncWidthDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class EventConsumersPageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public eventConsumerErrorDialog = new DialogModel();
    public eventConsumerError?: string;

    constructor(
        public readonly eventConsumersState: EventConsumersState,
    ) {
    }

    public ngOnInit() {
        this.eventConsumersState.load();

        this.subscriptions.add(
            timer(1000, 1000).pipe(
                switchMap(() => this.eventConsumersState.load(false, true))));
    }

    public reload() {
        this.eventConsumersState.load(true, false);
    }

    public showError(eventConsumer: EventConsumerDto) {
        this.eventConsumerError = eventConsumer.error;
        this.eventConsumerErrorDialog.show();
    }
}
