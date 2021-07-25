/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input } from '@angular/core';
import { TriggerForm } from '@app/shared';

@Component({
    selector: 'sqx-comment-trigger[triggerForm]',
    styleUrls: ['./comment-trigger.component.scss'],
    templateUrl: './comment-trigger.component.html',
})
export class CommentTriggerComponent {
    @Input()
    public triggerForm: TriggerForm;
}
