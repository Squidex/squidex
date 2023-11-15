/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { RuleElementMetadataDto, SafeHtmlPipe } from '@app/shared';

@Component({
    selector: 'sqx-rule-icon',
    styleUrls: ['./rule-icon.component.scss'],
    templateUrl: './rule-icon.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [NgIf, SafeHtmlPipe],
})
export class RuleIconComponent {
    @Input({ required: true })
    public element!: RuleElementMetadataDto;

    @Input()
    public size: 'sm' | 'md' | 'lg' = 'sm';
}
