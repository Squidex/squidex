/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';
import { SimulatedRuleEventDto } from '@app/shared';

@Pipe({
    name: 'sqxRuleClass',
    pure: true,
})
export class RuleClassPipe implements PipeTransform {
    public transform(value: string) {
        if (value === 'Retry' || value === 'Skipped') {
            return 'warning';
        } else if (value === 'Failed' || value === 'Cancelled') {
            return 'danger';
        } else if (value === 'Pending') {
            return 'secondary';
        } else {
            return value.toLowerCase();
        }
    }
}

@Pipe({
    name: 'sqxSimulatedRuleEventStatus',
    pure: true,
})
export class SimulatedRuleEventStatusPipe implements PipeTransform {
    public transform(value: SimulatedRuleEventDto) {
        if (value.error) {
            return 'Failed';
        } else if (value.skipReasons.length > 0) {
            return 'Skipped';
        } else {
            return 'Success';
        }
    }
}