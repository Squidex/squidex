/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { TriggerForm } from '@app/shared';

@Component({
    selector: 'sqx-asset-changed-trigger',
    styleUrls: ['./asset-changed-trigger.component.scss'],
    templateUrl: './asset-changed-trigger.component.html',
})
export class AssetChangedTriggerComponent {
    @Input()
    public trigger: any;

    @Input()
    public triggerForm: TriggerForm;
}
