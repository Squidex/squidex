/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { ActionForm } from '@app/shared';

@Component({
    selector: 'sqx-generic-action[actionForm]',
    styleUrls: ['./generic-action.component.scss'],
    templateUrl: './generic-action.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GenericActionComponent {
    @Input()
    public actionForm: ActionForm;
}
