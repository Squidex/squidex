/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, effect, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { ConfirmClickDirective, LayoutComponent, ListViewComponent, PagerComponent, Router2State, RuleElementDto, RuleEventDto, RuleEventsState, RulesService, ShortcutDirective, TitleComponent, TooltipDirective, TranslatePipe } from '@app/shared';
import { RuleEventComponent } from './rule-event.component';

@Component({
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
export class RuleEventsPageComponent {
    public readonly availableSteps;

    public readonly selectedEventId = signal<string | null>(null);

    constructor(
        public readonly ruleEventsRoute: Router2State,
        public readonly ruleEventsState: RuleEventsState,
        route: ActivatedRoute,
        rulesService: RulesService,
    ) {
        this.availableSteps = toSignal(rulesService.getSteps(), { initialValue: {} as Record<string, RuleElementDto> });

        effect(onCleanup => {
            const subscription = route.queryParams
                .subscribe(() => {
                    const initial =
                        this.ruleEventsRoute.mapTo(this.ruleEventsState)
                            .withPaging('rules', 30)
                            .withString('ruleId')
                            .withString('query')
                            .getInitial();

                    this.ruleEventsState.load(false, initial);
                });

            onCleanup(() => subscription.unsubscribe());
        });
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
        this.selectedEventId.update(x => x === id ? null : id);
    }
}
