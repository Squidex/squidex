/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { AppDto, fadeAnimation } from '@app/shared';

@Component({
    selector: 'sqx-github-card[app]',
    styleUrls: ['./github-card.component.scss'],
    templateUrl: './github-card.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GithubCardComponent {
    @Input()
    public app: AppDto;
}
