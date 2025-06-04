/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { SimulatedRuleEventDto, TranslatePipe, TypedSimpleChanges } from '@app/shared';
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
    @Input({ transform: booleanAttribute })
    public isLast = false;

    @Input()
    public event: SimulatedRuleEventDto | undefined | null;

    @Input()
    public errors: ReadonlyArray<string> | undefined | null;

    @Input()
    public text: string | undefined | null;

    public filteredErrors: string[] = [];

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.event || changes.errors) {
            const errors = this.errors;

            if (!errors) {
                this.filteredErrors = [];
                return;
            }

            const result = this.event?.skipReasons.filter(x => errors.includes(x)).map(x => `rules.simulation.error${x}`);

            if (result?.length === 0) {
                this.filteredErrors = [];
                return;
            }

            this.filteredErrors = result ?? [];
        }
    }
}
