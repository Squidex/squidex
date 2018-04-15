/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { AppsState, DialogService } from '@app/shared';

@Component({
    selector: 'sqx-more-page',
    styleUrls: ['./more-page.component.scss'],
    templateUrl: './more-page.component.html'
})
export class MorePageComponent {
    constructor(
        public readonly appsState: AppsState,
        private readonly dialogs: DialogService,
        private readonly router: Router
    ) {
    }

    public archiveApp() {
        this.appsState.delete(this.appsState.appName)
            .subscribe(() => {
                this.router.navigate(['/app']);
            }, error => {
                this.dialogs.notifyError(error);
            });
    }
}

