/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { AppDto } from '@app/shared';

@Component({
    selector: 'sqx-github-card[app]',
    styleUrls: ['./github-card.component.scss'],
    templateUrl: './github-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GithubCardComponent {
    @Input()
    public app!: AppDto;
}
