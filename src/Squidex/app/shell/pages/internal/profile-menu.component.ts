/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import {
    ApiUrlConfig,
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
export class ProfileMenuComponent implements OnDestroy, OnInit {
    private authenticationSubscription: Subscription;

    public modalMenu = new ModalView(false, true);

    public profileDisplayName = '';
    public profileId = '';

    public isAdmin = false;

    public profileUrl = this.apiUrl.buildUrl('/identity-server/account/profile');

    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public ngOnDestroy() {
        this.authenticationSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.authenticationSubscription =
            this.authService.userChanges.filter(user => !!user)
                .subscribe(user => {
                    this.profileId = user.id;
                    this.profileDisplayName = user.displayName;

                    this.isAdmin = user.isAdmin;
                });
    }

    public logout() {
        this.authService.logoutRedirect();
    }
}