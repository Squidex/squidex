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

    public login() {
        const redirectPath =
            this.route.snapshot.queryParams.redirectPath ||
            this.location.path();

        if (this.isIE()) {
            this.authService.loginRedirect(redirectPath);
        } else {
            this.authService.loginPopup(redirectPath)
                .subscribe({
                    next: path => {
                        path ||= '/app';

                        this.router.navigateByUrl(path);
                    },
                    error: () => {
                        this.showLoginError = true;
                    },
                });
        }
    }

    public isIE() {
        return !!navigator.userAgent.match(/Trident/g) || !!navigator.userAgent.match(/MSIE/g);
    }
}
