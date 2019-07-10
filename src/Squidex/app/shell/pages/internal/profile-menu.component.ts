/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';

import {
    ApiUrlConfig,
    AuthService,
    fadeAnimation,
    ModalModel,
    StatefulComponent,
    UIState
} from '@app/shared';

interface State {
    profileDisplayName: string;
    profileId: string;
    profileEmail: string;
    profileUrl: string;
}

@Component({
    selector: 'sqx-profile-menu',
    styleUrls: ['./profile-menu.component.scss'],
    templateUrl: './profile-menu.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileMenuComponent extends StatefulComponent<State> implements OnInit {
    public modalMenu = new ModalModel();

    constructor(changeDetector: ChangeDetectorRef, apiUrl: ApiUrlConfig,
        public readonly uiState: UIState,
        private readonly authService: AuthService
    ) {
        super(changeDetector, {
            profileDisplayName: '',
            profileEmail: '',
            profileId: '',
            profileUrl: apiUrl.buildUrl('/identity-server/account/profile')
        });
    }
    public ngOnInit() {
        this.own(
            this.authService.userChanges
                .subscribe(user => {
                    if (user) {
                        const profileId = user.id;
                        const profileEmail = user.email;
                        const profileDisplayName = user.displayName;

                        this.next(s => ({ ...s, profileId, profileEmail, profileDisplayName }));
                    }
                }));
    }

    public logout() {
        this.authService.logoutRedirect();
    }
}