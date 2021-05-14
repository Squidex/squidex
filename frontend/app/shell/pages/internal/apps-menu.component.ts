/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { AppDto, AppsState, DialogModel, fadeAnimation, ModalModel, UIState } from '@app/shared';

@Component({
    selector: 'sqx-apps-menu',
    styleUrls: ['./apps-menu.component.scss'],
    templateUrl: './apps-menu.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppsMenuComponent {
    public addAppDialog = new DialogModel();

    public appsMenu = new ModalModel();

    constructor(
        public readonly appsState: AppsState,
        public readonly uiState: UIState,
    ) {
    }

    public trackByApp(_index: number, app: AppDto) {
        return app.id;
    }
}
