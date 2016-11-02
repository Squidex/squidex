/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Common from '@angular/common';

import { AuthService } from './../../shared';

@Ng2.Component({
    selector: 'login',
    template
})
export class LoginPageComponent implements Ng2.OnInit {
    public showFailedError = false;

    constructor(
        private readonly authService: AuthService,
        private readonly location: Ng2Common.Location
    ) {
    }

    public ngOnInit() {
        this.authService.login().subscribe(
            () => {
                this.location.back();
            },
            () => {
                this.showFailedError = true;
            }
        );
    }
}