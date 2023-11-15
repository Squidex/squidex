/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ExternalLinkDirective, TranslatePipe } from '@app/framework';

@Component({
    standalone: true,
    selector: 'sqx-support-card',
    styleUrls: ['./support-card.component.scss'],
    templateUrl: './support-card.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ExternalLinkDirective,
        TranslatePipe,
    ],
})
export class SupportCardComponent {
}
