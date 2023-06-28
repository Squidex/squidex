/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Location } from '@angular/common';
import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService, UIOptions } from '@app/shared';

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
        private readonly uiOptions: UIOptions,
    ) {
    }

    public async login() {
        const redirectPath =
            this.route.snapshot.queryParams.redirectPath ||
            this.location.path();

        if (this.isInternetExplorer() || this.uiOptions.get('redirectToLogin')) {
            this.authService.loginRedirect(redirectPath);
            return;
        }

        try {
            let path = await this.authService.loginPopup(redirectPath);

            if (!path) {
                path = '/app';
            }

            const success = await this.router.navigateByUrl(path, { replaceUrl: true });

            if (!success) {
                this.router.navigate(['/app'], { replaceUrl: true });
            }
        } catch {
            this.router.navigate(['/'], { replaceUrl: true });
        }
    }

    public isInternetExplorer() {
        return !!navigator.userAgent.match(/Trident/g) || !!navigator.userAgent.match(/MSIE/g);
    }
}
