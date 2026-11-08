/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LayoutContainerDirective, TitleComponent, TranslatePipe, UIState } from '@app/shared';

@Component({
    selector: 'sqx-administration-area',
    styleUrls: ['./administration-area.component.scss'],
    templateUrl: './administration-area.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        LayoutContainerDirective,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        TitleComponent,
        TranslatePipe,
    ],
})
export class AdministrationAreaComponent {
    protected readonly uiState = inject(UIState);
}
