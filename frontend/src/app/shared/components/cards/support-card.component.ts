/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { ExternalLinkDirective, TranslatePipe } from '@app/framework';

@Component({
    selector: 'sqx-support-card',
    styleUrls: ['./support-card.component.scss'],
    templateUrl: './support-card.component.html',
    imports: [
        ExternalLinkDirective,
        TranslatePipe,
    ],
})
export class SupportCardComponent {
}
