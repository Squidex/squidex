/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';

import {
    ApiUrlConfig,
    AuthService,
    fadeAnimation,
    ModalModel
} from '@app/shared';

@Component({
    selector: 'sqx-profile-menu',
    styleUrls: ['./profile-menu.component.scss'],
    templateUrl: './profile-menu.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileMenuComponent implements OnDestroy, OnInit {
    private authenticationSubscription: Subscription;

    public modalMenu = new ModalModel();

    public profileDisplayName = '';
    public profileId = '';

    public profileUrl = this.apiUrl.buildUrl('/identity-server/account/profile');

    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig,
        private readonly changeDetector: ChangeDetectorRef
    ) {
    }

    public ngOnDestroy() {
        this.authenticationSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.authenticationSubscription =
            this.authService.userChanges.pipe(filter(user => !!user))
                .subscribe(user => {
                    this.profileId = user!.id;
                    this.profileDisplayName = user!.displayName;
					
                    this.changeDetector.markForCheck();
                });
    }

    public logout() {
        this.authService.logoutRedirect();
    }
}