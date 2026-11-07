/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RuleTriggerMetadataDto, SafeHtmlPipe } from '@app/shared';

@Component({
    selector: 'sqx-rule-icon',
    styleUrls: ['./rule-icon.component.scss'],
    templateUrl: './rule-icon.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        SafeHtmlPipe,
    ],
})
export class RuleIconComponent {
    /** The metadata of the trigger or step, which defines the icon. */
    public readonly elementInfo = input.required<RuleTriggerMetadataDto>();

    /** The size of the icon. */
    public readonly size = input<'sm' | 'md' | 'lg'>('sm');
}
