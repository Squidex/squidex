/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, effect, signal, inject } from '@angular/core';
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
    protected readonly eventConsumersState = inject(EventConsumersState);

    protected readonly eventConsumerErrorDialog = new DialogModel();
    protected readonly eventConsumerError = signal<string | undefined>(undefined);

    constructor() {
        this.eventConsumersState.load();

        effect(onCleanup => {
            const subscription =
                timer(1000, 1000).pipe(
                    switchMap(() => this.eventConsumersState.load(false, true)))
                    .subscribe();

            onCleanup(() => subscription.unsubscribe());
        });
    }

    protected reload() {
        this.eventConsumersState.load(true, false);
    }

    protected showError(eventConsumer: EventConsumerDto) {
        this.eventConsumerError.set(eventConsumer.error);
        this.eventConsumerErrorDialog.show();
    }
}
