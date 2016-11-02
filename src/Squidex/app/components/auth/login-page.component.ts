/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Router from '@angular/router';

import { AuthService } from './../../shared';

@Ng2.Component({
    selector: 'login',
    template: ''
})
export class LoginPageComponent implements Ng2.OnInit {
    public showError = false;

    constructor(
        private readonly authService: AuthService,
        private readonly router: Ng2Router.Router
    ) {
    }

    public ngOnInit() {
        this.authService.loginComplete().subscribe(
            () => {
        debugger;
                this.router.navigate(['/']);
            },
           e => {
        debugger;
                this.showError = true;
            }
        );
    }
}