/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Location } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService, FormHintComponent, TranslatePipe, UIOptions } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-home-page',
    styleUrls: ['./home-page.component.scss'],
    templateUrl: './home-page.component.html',
    imports: [
        FormHintComponent,
        TranslatePipe,
    ],
})
export class HomePageComponent {
    private readonly redirectToLogin = inject(UIOptions).value.redirectToLogin;

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

        if (this.isInternetExplorer() || this.redirectToLogin) {
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
