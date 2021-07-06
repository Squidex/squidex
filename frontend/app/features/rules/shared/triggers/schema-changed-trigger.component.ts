/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { TriggerForm } from '@app/shared';

@Component({
    selector: 'sqx-schema-changed-trigger',
    styleUrls: ['./schema-changed-trigger.component.scss'],
    templateUrl: './schema-changed-trigger.component.html',
})
export class SchemaChangedTriggerComponent {
    @Input()
    public trigger: any;

    @Input()
    public triggerForm: TriggerForm;
}
