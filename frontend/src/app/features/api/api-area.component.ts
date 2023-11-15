/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AppsState, ExternalLinkDirective, LayoutComponent, TitleComponent, TourStepDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-api-area',
    styleUrls: ['./api-area.component.scss'],
    templateUrl: './api-area.component.html',
    standalone: true,
    imports: [
        TitleComponent,
        LayoutComponent,
        TourStepDirective,
        RouterLink,
        RouterLinkActive,
        ExternalLinkDirective,
        RouterOutlet,
        TranslatePipe,
    ],
})
export class ApiAreaComponent {
    constructor(
        public readonly appsState: AppsState,
    ) {
    }
}
