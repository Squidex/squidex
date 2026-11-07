/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
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
    private readonly ruleEventsRoute = inject(Router2State);

    protected readonly ruleEventsState = inject(RuleEventsState);
    protected readonly availableSteps = toSignal(inject(RulesService).getSteps(), { initialValue: {} as Record<string, RuleElementDto> });
    protected readonly selectedEventId = signal<string | null>(null);

    constructor() {
        inject(ActivatedRoute).queryParams
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
