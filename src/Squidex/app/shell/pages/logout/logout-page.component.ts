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
    selector: 'sqx-logout',
    template: ''
})
export class LogoutPageComponent implements OnInit {
    constructor(
        private readonly auth: AuthService,
        private readonly router: Router
    ) {
    }

    public ngOnInit() {
        this.auth.logoutRedirectComplete()
            .subscribe(
                () => {
                    this.router.navigate(['/'], { replaceUrl: true });
                },
                () => {
                    this.router.navigate(['/'], { replaceUrl: true });
                }
            );
    }
}