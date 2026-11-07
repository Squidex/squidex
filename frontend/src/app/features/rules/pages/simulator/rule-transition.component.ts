/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { SimulatedRuleEventDto, TranslatePipe } from '@app/shared';
import { HistoryStepComponent } from '../../shared/history-step.component';

@Component({
    selector: 'sqx-rule-transition',
    styleUrls: ['./rule-transition.component.scss'],
    templateUrl: './rule-transition.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        HistoryStepComponent,
        TranslatePipe,
    ],
})
export class RuleTransitionComponent {
    /** Marks the transition as the last step in the history. */
    public readonly isLast = input(false, { transform: booleanAttribute });

    /** The simulated event, which provides the skip reasons. */
    public readonly event = input<SimulatedRuleEventDto | undefined | null>();

    /** The skip reasons that are relevant for this transition. */
    public readonly errors = input<ReadonlyArray<string> | undefined | null>();

    /** The text of the transition. */
    public readonly text = input<string | undefined | null>();

    public readonly filteredErrors = computed(() => {
        const errors = this.errors();
        if (!errors) {
            return [];
        }

        return this.event()?.skipReasons.filter(x => errors.includes(x)).map(x => `rules.simulation.error${x}`) ?? [];
    });
}
