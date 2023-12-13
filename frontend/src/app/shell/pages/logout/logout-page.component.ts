/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-logout',
    template: '',
})
export class LogoutPageComponent implements OnInit {
    constructor(
        private readonly authService: AuthService,
        private readonly router: Router,
    ) {
    }

    public async ngOnInit() {
        try {
            const path = await this.authService.logoutRedirectComplete() || '/';

            this.router.navigateByUrl(addQuery(path, 'logout', 'true'), { replaceUrl: true });
        } catch {
            this.router.navigate(['/'], { replaceUrl: true, queryParams: { logout: true } });
        }
    }
}

function addQuery(path: string, key: string, value: string) {
    if (path.indexOf('?') >= 0) {
        return `${path}&${key}=${value}`;
    } else {
        return `${path}?${key}=${value}`;
    }
}
