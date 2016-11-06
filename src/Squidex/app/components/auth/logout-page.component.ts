/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Router from '@angular/router';

import { AuthService } from 'shared';

@Ng2.Component({
    selector: 'logout',
    template: ''
})
export class LogoutPageComponent implements Ng2.OnInit {
    constructor(
        private readonly auth: AuthService,
        private readonly router: Ng2Router.Router
    ) {
    }

    public ngOnInit() {
        this.auth.logoutComplete().subscribe(
            () => {
                this.router.navigate(['/'], { replaceUrl: true });
            },
            () => {
                this.router.navigate(['/'], { replaceUrl: true });
            }
        );
    }
}