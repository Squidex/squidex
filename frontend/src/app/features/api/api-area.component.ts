/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AppsState, ExternalLinkDirective, LayoutComponent, TitleComponent, TourStepDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-api-area',
    styleUrls: ['./api-area.component.scss'],
    templateUrl: './api-area.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ExternalLinkDirective,
        LayoutComponent,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        TitleComponent,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class ApiAreaComponent {
    protected readonly appsState = inject(AppsState);
}
