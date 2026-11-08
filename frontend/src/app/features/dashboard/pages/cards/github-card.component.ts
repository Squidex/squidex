/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { ExternalLinkDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-github-card',
    styleUrls: ['./github-card.component.scss'],
    templateUrl: './github-card.component.html',
    imports: [
        ExternalLinkDirective,
        TranslatePipe,
    ],
})
export class GithubCardComponent {
}
