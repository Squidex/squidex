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
    selector: 'login',
    template
})
export class LoginPageComponent implements Ng2.OnInit {
    public showError = false;

    constructor(
        private readonly authService: AuthService,
        private readonly router: Ng2Router.Router,
        private readonly title: TitleService,
    ) {
    }

    public ngOnInit() {
        this.authService.loginComplete().subscribe(
            () => {
                this.router.navigate(['/'], { replaceUrl: true });
            },
           e => {
                this.title.setTitle('Login failed');
                
                this.showError = true;
            }
        );
    }
}