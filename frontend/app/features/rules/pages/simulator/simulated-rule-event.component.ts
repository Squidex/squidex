/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { SimulatedRuleEventDto } from '@app/shared';

@Component({
    selector: '[sqxSimulatedRuleEvent]',
    styleUrls: ['./simulated-rule-event.component.scss'],
    templateUrl: './simulated-rule-event.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SimulatedRuleEventComponent {
    @Input('sqxSimulatedRuleEvent')
    public event: SimulatedRuleEventDto;

    @Input()
    public expanded: boolean;

    @Output()
    public expandedChange = new EventEmitter<any>();

    public get data() {
        let result = this.event.actionData;

        if (result) {
            try {
                result = JSON.stringify(JSON.parse(result), null, 2);
            } catch {
                result = this.event.actionData;
            }
        }

        return result;
    }

    public get status() {
        if (this.event.error) {
            return 'Failed';
        } else if (this.event.skipReason !== 'None') {
            return 'Skipped';
        } else {
            return 'Success';
        }
    }

    public get statusClass() {
        if (this.event.error) {
            return 'danger';
        } else if (this.event.skipReason !== 'None') {
            return 'warning';
        } else {
            return 'success';
        }
    }
}
