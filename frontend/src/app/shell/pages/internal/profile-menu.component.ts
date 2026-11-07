/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiUrlConfig, AuthService, Cookies, DropdownMenuComponent, ExternalLinkDirective, ModalDirective, ModalModel, ModalPlacementDirective, StopClickDirective, ThemeMode, ThemeService, TranslatePipe, UILanguages, UIOptions, UIState, UserIdPicturePipe } from '@app/shared';

@Component({
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
export class ProfileMenuComponent {
    public readonly modalMenu = new ModalModel();

    public readonly language = inject(UIOptions).value.culture;
    public readonly languages = UILanguages.ALL;

    public readonly themeModes: ReadonlyArray<{ mode: ThemeMode; label: string }> = [
        { mode: 'light', label: 'common.themeLight' },
        { mode: 'dark', label: 'common.themeDark' },
        { mode: 'system', label: 'common.themeSystem' },
    ];

    public readonly profileId = signal('');
    public readonly profileEmail = signal('');
    public readonly profileUrl: string;

    public readonly submenu = signal<'language' | 'theme' | null>(null);

    constructor(apiUrl: ApiUrlConfig,
        public readonly uiState: UIState,
        public readonly uiOptions: UIOptions,
        public readonly authService: AuthService,
        public readonly themeService: ThemeService,
    ) {
        this.profileUrl = apiUrl.buildUrl('/identity-server/account/profile');

        effect(onCleanup => {
            const subscription =
                this.authService.userChanges
                    .subscribe(user => {
                        if (user) {
                            this.profileId.set(user.id);
                            this.profileEmail.set(user.email);
                        }
                    });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    public changeLanguage(code: string) {
        Cookies.replace('.AspNetCore.Culture', `c=${code}|uic=${code}`, 365);

        location.reload();
    }

    public changeTheme(mode: ThemeMode) {
        this.themeService.setMode(mode);
    }

    public toggleProfile() {
        this.modalMenu.toggle();

        this.submenu.set(null);
    }

    public toggleSubmenu(submenu: 'language' | 'theme') {
        this.submenu.update(x => x === submenu ? null : submenu);
    }

    public logout() {
        this.authService.logoutRedirect('/');
    }
}
