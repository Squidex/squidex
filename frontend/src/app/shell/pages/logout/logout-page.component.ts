/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@app/shared';

@Component({
    selector: 'sqx-logout',
    template: '',
})
export class LogoutPageComponent {
    private readonly authService = inject(AuthService);
    private readonly router = inject(Router);

    constructor() {
        this.completeRedirect();
    }

    private async completeRedirect() {
        try {
            const path = await this.authService.logoutRedirectComplete() || '/';

            const separator = path.indexOf('?') >= 0 ? '&' : '?';

            this.router.navigateByUrl(`${path}${separator}logout=true`, { replaceUrl: true });
        } catch {
            this.router.navigate(['/'], { replaceUrl: true, queryParams: { logout: true } });
        }
    }
}
