/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { AuthService } from 'shared';

@Component({
    selector: 'sqx-login',
    template: ''
})
export class LoginPageComponent implements OnInit {
    constructor(
        private readonly auth: AuthService,
        private readonly router: Router
    ) {
    }

    public ngOnInit() {
        this.auth.loginRedirectComplete().subscribe(
            () => {
                this.router.navigate(['/app'], { replaceUrl: true });
            },
            () => {
                this.router.navigate(['/'], { replaceUrl: true });
            }
        );
    }
}