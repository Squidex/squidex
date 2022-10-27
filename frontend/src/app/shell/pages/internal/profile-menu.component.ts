/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ApiUrlConfig, AuthService, Cookies, ModalModel, StatefulComponent, UILanguages, UIOptions, UIState } from '@app/shared';

interface State {
    // The display name of the user.
    profileDisplayName: string;

    // The id of the user.
    profileId: string;

    // The email address of the user.
    profileEmail: string;

    // The url to the user profile.
    profileUrl: string;

    // True when the submenu should be open.
    showSubmenu: boolean;
}

@Component({
    selector: 'sqx-profile-menu',
    styleUrls: ['./profile-menu.component.scss'],
    templateUrl: './profile-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfileMenuComponent extends StatefulComponent<State> implements OnInit {
    public modalMenu = new ModalModel();

    public language = this.uiOptions.get('culture');
    public languages = UILanguages.ALL;

    constructor(changeDetector: ChangeDetectorRef, apiUrl: ApiUrlConfig,
        public readonly uiState: UIState,
        public readonly uiOptions: UIOptions,
        public readonly authService: AuthService,
    ) {
        super(changeDetector, {
            profileDisplayName: '',
            profileEmail: '',
            profileId: '',
            profileUrl: apiUrl.buildUrl('/identity-server/account/profile'),
            showSubmenu: false,
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

                        this.next({
                            profileId,
                            profileEmail,
                            profileDisplayName,
                        });
                    }
                }));
    }

    public changeLanguage(code: string) {
        Cookies.replace('.AspNetCore.Culture', `c=${code}|uic=${code}`, 365);

        location.reload();
    }

    public toggleProfile() {
        this.modalMenu.toggle();

        this.next(s => ({
            ...s,
            showSubmenu: false,
        }));
    }

    public toggleSubmenu() {
        this.next(s => ({
            ...s,
            showSubmenu: !s.showSubmenu,
        }));
    }

    public logout() {
        this.authService.logoutRedirect('/');
    }
}
