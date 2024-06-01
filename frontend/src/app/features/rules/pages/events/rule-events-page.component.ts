/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ConfirmClickDirective, LayoutComponent, ListViewComponent, PagerComponent, Router2State, RuleEventDto, RuleEventsState, ShortcutDirective, Subscriptions, TitleComponent, TooltipDirective, TranslatePipe } from '@app/shared';
import { RuleEventComponent } from './rule-event.component';

@Component({
    standalone: true,
    selector: 'sqx-rule-events-page',
    styleUrls: ['./rule-events-page.component.scss'],
    templateUrl: './rule-events-page.component.html',
    providers: [
        Router2State,
    ],
    imports: [
        AsyncPipe,
        ConfirmClickDirective,
        LayoutComponent,
        ListViewComponent,
        PagerComponent,
        RuleEventComponent,
        ShortcutDirective,
        TitleComponent,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class RuleEventsPageComponent implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public selectedEventId: string | null = null;

    constructor(
        private readonly route: ActivatedRoute,
        public readonly ruleEventsRoute: Router2State,
        public readonly ruleEventsState: RuleEventsState,
    ) {
    }

    public ngOnInit() {
        this.subscriptions.add(
            this.route.queryParams
                .subscribe(() => {
                    const initial =
                        this.ruleEventsRoute.mapTo(this.ruleEventsState)
                            .withPaging('rules', 30)
                            .withString('ruleId')
                            .withString('query')
                            .getInitial();

                    this.ruleEventsState.load(false, initial);
                }));
    }

    public reload() {
        this.ruleEventsState.load(true);
    }

    public enqueue(event: RuleEventDto) {
        this.ruleEventsState.enqueue(event);
    }

    public cancelAll() {
        this.ruleEventsState.cancelAll();
    }

    public cancel(event: RuleEventDto) {
        this.ruleEventsState.cancel(event);
    }

    public selectEvent(id: string) {
        if (this.selectedEventId === id) {
            this.selectedEventId = null;
        } else {
            this.selectedEventId = id;
        }
    }
}
