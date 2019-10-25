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
    selector: 'sqx-login',
    template: ''
})
export class LoginPageComponent implements OnInit {
    constructor(
        private readonly authService: AuthService,
        private readonly router: Router
    ) {
    }

    public ngOnInit() {
        this.authService.loginRedirectComplete()
            .subscribe(
                () => {
                    this.router.navigate(['/app'], { replaceUrl: true });
                },
                () => {
                    this.router.navigate(['/'], { replaceUrl: true });
                }
            );
    }
}