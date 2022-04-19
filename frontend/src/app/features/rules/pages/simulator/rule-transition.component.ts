/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { SimulatedRuleEventDto } from '@app/shared';

@Component({
    selector: 'sqx-rule-transition',
    styleUrls: ['./rule-transition.component.scss'],
    templateUrl: './rule-transition.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RuleTransitionComponent implements OnChanges {
    @Input()
    public event: SimulatedRuleEventDto | undefined | null;

    @Input()
    public errors: ReadonlyArray<string> | undefined | null;

    @Input()
    public text: string | undefined | null;

    public filteredErrors?: string[] | null;

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['event'] || changes['errors']) {
            const errors = this.errors;

            if (!errors) {
                this.filteredErrors = null;
                return;
            }

            const result = this.event?.skipReasons.filter(x => errors.includes(x)).map(x => `rules.simulation.error${x}`);

            if (result?.length === 0) {
                this.filteredErrors = null;
                return;
            }

            this.filteredErrors = result;
        }
    }
}
