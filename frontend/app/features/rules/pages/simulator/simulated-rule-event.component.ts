/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { SimulatedRuleEventDto } from '@app/shared';

const ERRORS_AFTER_EVENT = [
    'ConditionPrecheckDoesNotMatch',
    'Disabled',
    'FromRule',
    'NoAction',
    'NoTrigger',
    'TooOld',
    'WrongEvent',
    'WrongEventForTrigger',
];

const ERRORS_AFTER_ENRICHED_EVENT = [
    'ConditionDoesNotMatch',
];

const ERRORS_FAILED = [
    'Failed',
];

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

    public errorsAfterEvent = ERRORS_AFTER_EVENT;
    public errorsAfterEnrichedEvent = ERRORS_AFTER_ENRICHED_EVENT;
    public errorsFailed = ERRORS_FAILED;

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
        } else if (this.event.skipReasons.length > 0) {
            return 'Skipped';
        } else {
            return 'Success';
        }
    }

    public get statusClass() {
        if (this.event.error) {
            return 'danger';
        } else if (this.event.skipReasons.length > 0) {
            return 'warning';
        } else {
            return 'success';
        }
    }
}
