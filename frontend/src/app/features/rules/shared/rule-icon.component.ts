/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { RuleElementMetadataDto } from '@app/shared';

@Component({
    selector: 'sqx-rule-icon',
    styleUrls: ['./rule-icon.component.scss'],
    templateUrl: './rule-icon.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RuleIconComponent {
    @Input({ required: true })
    public element!: RuleElementMetadataDto;

    @Input()
    public size: 'sm' | 'md' | 'lg' = 'sm';
}
