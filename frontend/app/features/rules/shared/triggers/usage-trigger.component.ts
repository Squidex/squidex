/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { TriggerForm } from '@app/shared';

@Component({
    selector: 'sqx-usage-trigger[triggerForm]',
    styleUrls: ['./usage-trigger.component.scss'],
    templateUrl: './usage-trigger.component.html',
})
export class UsageTriggerComponent {
    @Input()
    public triggerForm: TriggerForm;
}
