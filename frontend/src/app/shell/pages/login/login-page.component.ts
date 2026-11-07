/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@app/shared';

@Component({
    selector: 'sqx-login',
    template: '',
})
export class LoginPageComponent {
    constructor(
        private readonly authService: AuthService,
        private readonly router: Router,
    ) {
        this.completeRedirect();
    }

    private async completeRedirect() {
        try {
            const path = await this.authService.loginRedirectComplete();

            this.router.navigateByUrl(path || '/app', { replaceUrl: true });
        } catch {
            this.router.navigate(['/'], { replaceUrl: true });
        }
    }
}
