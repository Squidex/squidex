/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LayoutContainerDirective, TitleComponent, TranslatePipe, UIState } from '@app/shared';

@Component({
    selector: 'sqx-administration-area',
    styleUrls: ['./administration-area.component.scss'],
    templateUrl: './administration-area.component.html',
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
    constructor(
        protected readonly uiState: UIState,
    ) {
    }
}
