/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiUrlConfig, AuthService, Cookies, DropdownMenuComponent, ExternalLinkDirective, ModalDirective, ModalModel, ModalPlacementDirective, StatefulComponent, StopClickDirective, Subscriptions, TranslatePipe, UILanguages, UIOptions, UIState, UserIdPicturePipe } from '@app/shared';

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
    standalone: true,
    selector: 'sqx-profile-menu',
    styleUrls: ['./profile-menu.component.scss'],
    templateUrl: './profile-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        DropdownMenuComponent,
        ExternalLinkDirective,
        ModalDirective,
        ModalPlacementDirective,
        RouterLink,
        StopClickDirective,
        TranslatePipe,
        UserIdPicturePipe,
    ],
})
export class ProfileMenuComponent extends StatefulComponent<State> implements OnInit {
    private readonly subscriptions = new Subscriptions();

    public readonly modalMenu = new ModalModel();

    public readonly language = inject(UIOptions).value.culture;
    public readonly languages = UILanguages.ALL;

    constructor(apiUrl: ApiUrlConfig,
        public readonly uiState: UIState,
        public readonly uiOptions: UIOptions,
        public readonly authService: AuthService,
    ) {
        super({
            profileDisplayName: '',
            profileEmail: '',
            profileId: '',
            profileUrl: apiUrl.buildUrl('/identity-server/account/profile'),
            showSubmenu: false,
        });
    }

    public ngOnInit() {
        this.subscriptions.add(
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
