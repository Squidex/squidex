/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { AuthService, TitleService } from 'shared';

@Component({
    selector: 'home-page',
    styleUrls: ['./home-page.component.scss'],
    templateUrl: './home-page.component.html'
})
export class HomePageComponent implements OnInit {
    public showLoginError = false;

    constructor(
        private readonly auth: AuthService,
        private readonly title: TitleService,
        private readonly router: Router
    ) {
    }

    public ngOnInit() {
        this.title.setTitle('Home');
    }

    public login() {
        this.auth.loginPopup()
            .subscribe(() => {
                this.router.navigate(['/app']);
            }, ex => {
                this.title.setTitle('Login failed');

                this.showLoginError = true;
            });
    }
}