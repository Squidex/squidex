/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgIf } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LayoutContainerDirective, TitleComponent, TranslatePipe, UIState } from '@app/shared';

@Component({
    selector: 'sqx-administration-area',
    styleUrls: ['./administration-area.component.scss'],
    templateUrl: './administration-area.component.html',
    standalone: true,
    imports: [
        TitleComponent,
        NgIf,
        RouterLink,
        RouterLinkActive,
        LayoutContainerDirective,
        RouterOutlet,
        AsyncPipe,
        TranslatePipe,
    ],
})
export class AdministrationAreaComponent {
    constructor(
        public readonly uiState: UIState,
    ) {
    }
}
