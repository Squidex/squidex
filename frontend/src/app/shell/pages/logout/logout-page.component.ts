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
    selector: 'sqx-logout',
    template: '',
})
export class LogoutPageComponent implements OnInit {
    constructor(
        private readonly authService: AuthService,
        private readonly router: Router,
    ) {
    }

    public ngOnInit() {
        this.authService.logoutRedirectComplete()
            .subscribe({
                next: path => {
                    path ||= '/';

                    this.router.navigateByUrl(path, { replaceUrl: true });
                },
                error: () => {
                    this.router.navigate(['/'], { replaceUrl: true });
                },
            });
    }
}
