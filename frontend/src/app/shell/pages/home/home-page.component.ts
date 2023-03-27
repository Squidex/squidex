/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Location } from '@angular/common';
import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '@app/shared';

@Component({
    selector: 'sqx-home-page',
    styleUrls: ['./home-page.component.scss'],
    templateUrl: './home-page.component.html',
})
export class HomePageComponent {
    public showLoginError = false;

    constructor(
        private readonly authService: AuthService,
        private readonly location: Location,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
    ) {
    }

    public async login() {
        const redirectPath =
            this.route.snapshot.queryParams.redirectPath ||
            this.location.path();

        if (this.isInternetExplorer()) {
            this.authService.loginRedirect(redirectPath);
            return;
        }

        try {
            const path = await this.authService.loginPopup(redirectPath);

            this.router.navigateByUrl(path || '/app', { replaceUrl: true });
        } catch {
            this.router.navigate(['/'], { replaceUrl: true });
        }
    }

    public isInternetExplorer() {
        return !!navigator.userAgent.match(/Trident/g) || !!navigator.userAgent.match(/MSIE/g);
    }
}
