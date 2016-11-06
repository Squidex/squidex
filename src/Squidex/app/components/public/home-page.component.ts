/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Router from '@angular/router';

import { AuthService, TitleService } from 'shared';

@Ng2.Component({
    selector: 'not-found',
    template
})
export class HomePageComponent implements Ng2.OnInit {
    public showLoginError = false;

    constructor(
        private readonly auth: AuthService,
        private readonly title: TitleService,
        private readonly router: Ng2Router.Router,
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