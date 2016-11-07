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
export class ProfileMenuComponent implements Ng2.OnInit, Ng2.OnDestroy {
    private authenticationSubscription: any | null;

    public modalMenu = new ModalView(false, true);

    public profileDisplayName = '';
    public profilePictureUrl = '';

    constructor(
        private readonly auth: AuthService
    ) {
    }

    public ngOnInit() {
        this.authenticationSubscription =
            this.auth.isAuthenticatedChanges.subscribe(() => {
                const user = this.auth.user;

                if (user) {
                    this.profilePictureUrl = user.pictureUrl;
                    this.profileDisplayName = user.displayName;
                }
            });
    }

    public ngOnDestroy() {
        this.authenticationSubscription.unsubscribe();
    }

    public logout() {
        this.auth.logout();
    }
}