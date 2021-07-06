/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { RuleEventDto } from '@app/shared';

@Component({
    selector: '[sqxRuleEvent]',
    styleUrls: ['./rule-event.component.scss'],
    templateUrl: './rule-event.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RuleEventComponent {
    @Input('sqxRuleEvent')
    public event: RuleEventDto;

    @Input()
    public expanded: boolean;

    @Output()
    public expandedChange = new EventEmitter<any>();

    @Output()
    public enqueue = new EventEmitter<any>();

    @Output()
    public cancel = new EventEmitter<any>();

    public get jobResultClass() {
        return getClass(this.event.jobResult);
    }

    public get resultClass() {
        return getClass(this.event.result);
    }
}

function getClass(result: string) {
    if (result === 'Retry') {
        return 'warning';
    } else if (result === 'Failed' || result === 'Cancelled') {
        return 'danger';
    } else if (result === 'Pending') {
        return 'secondary';
    } else {
        return result.toLowerCase();
    }
}
