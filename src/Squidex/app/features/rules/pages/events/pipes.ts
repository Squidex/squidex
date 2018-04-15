/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'sqxRuleEventBadgeClass',
    pure: true
})
export class RuleEventBadgeClassPipe implements PipeTransform {
    public transform(status: string) {
        if (status === 'Retry') {
            return 'warning';
        } else if (status === 'Failed') {
            return 'danger';
        } else if (status === 'Pending') {
            return 'secondary';
        } else {
            return status.toLowerCase();
        }
    }
}