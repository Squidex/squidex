/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject, signal } from '@angular/core';
import { rxResource, takeUntilDestroyed } from '@angular/core/rxjs-interop';
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
    private readonly route = inject(ActivatedRoute);
    private readonly ruleEventsRoute = inject(Router2State);
    private readonly rulesService = inject(RulesService);

    protected readonly ruleEventsState = inject(RuleEventsState);

    protected readonly selectedEventId = signal<string | null>(null);

    protected readonly availableSteps = rxResource({
        stream: () => this.rulesService.getSteps(),
        defaultValue: {} as Record<string, RuleElementDto>,
    });

    constructor() {
        this.route.queryParams
            .pipe(takeUntilDestroyed())
            .subscribe(() => {
                const initial =
                    this.ruleEventsRoute.mapTo(this.ruleEventsState)
                        .withPaging('rules', 30)
                        .withString('ruleId')
                        .withString('query')
                        .getInitial();

                this.ruleEventsState.load(false, initial);
            });
    }

    protected reload() {
        this.ruleEventsState.load(true);
    }

    protected enqueue(event: RuleEventDto) {
        this.ruleEventsState.enqueue(event);
    }

    protected cancelAll() {
        this.ruleEventsState.cancelAll();
    }

    protected cancel(event: RuleEventDto) {
        this.ruleEventsState.cancel(event);
    }

    protected selectEvent(id: string) {
        this.selectedEventId.update(x => x === id ? null : id);
    }
}
