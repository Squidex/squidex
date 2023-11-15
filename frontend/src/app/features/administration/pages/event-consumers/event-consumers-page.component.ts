/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { DialogModel, LayoutComponent, ListViewComponent, ModalDialogComponent, ModalDirective, ShortcutDirective, SidebarMenuDirective, Subscriptions, SyncWidthDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { EventConsumerDto, EventConsumersState } from '../../internal';
import { EventConsumerComponent } from './event-consumer.component';

@Component({
    selector: 'sqx-event-consumers-page',
    styleUrls: ['./event-consumers-page.component.scss'],
    templateUrl: './event-consumers-page.component.html',
    standalone: true,
    imports: [
        TitleComponent,
        LayoutComponent,
        TooltipDirective,
        ShortcutDirective,
        ListViewComponent,
        SyncWidthDirective,
        NgFor,
        EventConsumerComponent,
        SidebarMenuDirective,
        RouterLink,
        RouterLinkActive,
        TourStepDirective,
        RouterOutlet,
        ModalDirective,
        ModalDialogComponent,
        AsyncPipe,
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

        this.subscriptions.add(timer(1000, 1000).pipe(switchMap(() => this.eventConsumersState.load(false, true))));
    }

    public reload() {
        this.eventConsumersState.load(true, false);
    }

    public trackByEventConsumer(_index: number, es: EventConsumerDto) {
        return es.name;
    }

    public showError(eventConsumer: EventConsumerDto) {
        this.eventConsumerError = eventConsumer.error;
        this.eventConsumerErrorDialog.show();
    }
}
