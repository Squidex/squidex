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
    selector: 'sqx-login',
    template: '',
})
export class LoginPageComponent implements OnInit {
    constructor(
        private readonly authService: AuthService,
        private readonly router: Router,
    ) {
    }

    public async ngOnInit() {
        try {
            const path = await this.authService.loginRedirectComplete();

            this.router.navigateByUrl(path || '/app', { replaceUrl: true });
        } catch {
            this.router.navigate(['/'], { replaceUrl: true });
        }
    }
}
