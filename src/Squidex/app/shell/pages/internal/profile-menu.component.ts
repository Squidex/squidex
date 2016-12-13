/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';

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
    private authenticationSubscription: any | null = null;

    public modalMenu = new ModalView(false, true);

    public profileDisplayName = '';
    public profilePictureUrl = '';

    constructor(
        private readonly auth: AuthService
    ) {
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

    public ngOnDestroy() {
        this.authenticationSubscription.unsubscribe();
    }

    public logout() {
        this.auth.logoutRedirect();
    }
}