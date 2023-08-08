/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { RuleElementMetadataDto } from '@app/shared';

@Component({
    selector: 'sqx-rule-element',
    styleUrls: ['./rule-element.component.scss'],
    templateUrl: './rule-element.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RuleElementComponent {
    @Input({ required: true })
    public type!: string;

    @Input({ required: true })
    public element!: RuleElementMetadataDto;

    @Input({ transform: booleanAttribute })
    public isSmall?: boolean | null = true;

    @Input({ transform: booleanAttribute })
    public disabled = false;
}
