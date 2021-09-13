/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { SimulatedRuleEventDto } from '@app/shared';

@Component({
    selector: 'sqx-rule-transition',
    styleUrls: ['./rule-transition.component.scss'],
    templateUrl: './rule-transition.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RuleTransitionComponent {
    @Input()
    public event: SimulatedRuleEventDto | undefined | null;

    @Input()
    public errors: ReadonlyArray<string> | undefined | null;

    @Input()
    public text: string | undefined | null;

    public get filteredErrors() {
        const errors = this.errors;

        if (!errors) {
            return null;
        }

        const result = this.event?.skipReasons.filter(x => errors.indexOf(x) >= 0).map(x => `rules.simulation.error${x}`);

        if (result?.length === 0) {
            return null;
        }

        return result;
    }
}
