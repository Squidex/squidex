/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ExternalLinkDirective, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-github-card',
    styleUrls: ['./github-card.component.scss'],
    templateUrl: './github-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ExternalLinkDirective,
        TranslatePipe,
    ],
})
export class GithubCardComponent {
}
