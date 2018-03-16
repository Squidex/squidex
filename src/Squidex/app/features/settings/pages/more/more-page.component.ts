/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { AppContext } from 'shared';

@Component({
    selector: 'sqx-more-page',
    styleUrls: ['./more-page.component.scss'],
    templateUrl: './more-page.component.html',
    providers: [
        AppContext
    ]
})
export class MorePageComponent {
    constructor(public readonly ctx: AppContext,
        private readonly router: Router
    ) {
    }

    public archiveApp() {
        this.ctx.appsStore.deleteApp(this.ctx.appName)
            .subscribe(() => {
                this.router.navigate(['/app']);
            }, error => {
                this.ctx.notifyError(error);
            });
    }
}

