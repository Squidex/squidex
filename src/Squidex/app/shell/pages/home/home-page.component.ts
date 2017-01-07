/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component } from '@angular/core';
import { Router } from '@angular/router';

import { AuthService } from 'shared';

@Component({
    selector: 'home-page',
    styleUrls: ['./home-page.component.scss'],
    templateUrl: './home-page.component.html'
})
export class HomePageComponent {
    public showLoginError = false;

    constructor(
        private readonly auth: AuthService,
        private readonly router: Router
    ) {
    }

    public login() {
        this.auth.loginPopup()
            .subscribe(() => {
                this.router.navigate(['/app']);
            }, ex => {
                this.showLoginError = true;
            });
    }
}