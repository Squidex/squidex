/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, effect, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { timer } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { DialogModel, EventConsumerDto, LayoutComponent, ListViewComponent, ModalDialogComponent, ModalDirective, ShortcutDirective, SidebarMenuDirective, SyncWidthDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { EventConsumersState } from '../../internal';
import { EventConsumerComponent } from './event-consumer.component';

@Component({
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
export class EventConsumersPageComponent {
    public readonly eventConsumerErrorDialog = new DialogModel();
    public readonly eventConsumerError = signal<string | undefined>(undefined);

    constructor(
        public readonly eventConsumersState: EventConsumersState,
    ) {
        this.eventConsumersState.load();

        effect(onCleanup => {
            const subscription =
                timer(1000, 1000).pipe(
                    switchMap(() => this.eventConsumersState.load(false, true)))
                    .subscribe();

            onCleanup(() => subscription.unsubscribe());
        });
    }

    public reload() {
        this.eventConsumersState.load(true, false);
    }

    public showError(eventConsumer: EventConsumerDto) {
        this.eventConsumerError.set(eventConsumer.error);
        this.eventConsumerErrorDialog.show();
    }
}
