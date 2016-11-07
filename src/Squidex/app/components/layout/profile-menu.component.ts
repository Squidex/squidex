/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import {
    AuthService,
    fadeAnimation,
    ModalView
} from 'shared';

@Ng2.Component({
    selector: 'sqx-profile-menu',
    styles,
    template,
    animations: [
        fadeAnimation()
    ]
})
export class ProfileMenuComponent {
    public modalMenu = new ModalView();

    public displayName
        = this.auth.isAuthenticatedChanges.map(t => t ? this.auth.user.displayName : null);

    public pictureUrl
        = this.auth.isAuthenticatedChanges.map(t => t ? this.auth.user.pictureUrl : null);

    constructor(
        private readonly auth: AuthService
    ) {
    }

    public logout() {
        this.auth.logout();
    }
}