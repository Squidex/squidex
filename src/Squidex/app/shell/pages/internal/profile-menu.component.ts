/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import {
    AuthService,
    fadeAnimation,
    ModalView
} from 'shared';

@Component({
    selector: 'sqx-profile-menu',
    styleUrls: ['./profile-menu.component.scss'],
    templateUrl: './profile-menu.component.html',
    animations: [
        fadeAnimation
    ]
})
export class ProfileMenuComponent implements OnInit, OnDestroy {
    private authenticationSubscription: Subscription;

    public modalMenu = new ModalView(false, true);

    public profileDisplayName = '';
    public profilePictureUrl = '';

    constructor(
        private readonly auth: AuthService
    ) {
    }

    public ngOnDestroy() {
        this.authenticationSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.authenticationSubscription =
            this.auth.isAuthenticated.subscribe(() => {
                const user = this.auth.user;

                if (user) {
                    this.profilePictureUrl = user.pictureUrl;
                    this.profileDisplayName = user.displayName;
                }
            });
    }

    public logout() {
        this.auth.logoutRedirect();
    }
}