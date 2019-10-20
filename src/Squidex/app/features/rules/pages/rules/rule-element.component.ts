/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

import { RuleElementDto } from '@app/shared';

@Component({
    selector: 'sqx-rule-element',
    styleUrls: ['./rule-element.component.scss'],
    templateUrl: './rule-element.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RuleElementComponent {
    @Input()
    public type: string;

    @Input()
    public element: RuleElementDto;

    @Input()
    public isSmall = true;

    @Input()
    public disabled = false;
}