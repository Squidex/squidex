/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiUrlConfig, AuthService, Cookies, DropdownMenuComponent, ExternalLinkDirective, ModalDirective, ModalModel, ModalPlacementDirective, StopClickDirective, ThemeMode, ThemeService, TranslatePipe, UILanguages, UIOptions, UIState, UserIdPicturePipe } from '@app/shared';

@Component({
    selector: 'sqx-profile-menu',
    styleUrls: ['./profile-menu.component.scss'],
    templateUrl: './profile-menu.component.html',
    imports: [
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
    private readonly apiUrlConfig = inject(ApiUrlConfig);
    private readonly authService = inject(AuthService);
    private readonly uiOptions = inject(UIOptions);

    protected readonly uiState = inject(UIState);

    protected readonly themeService = inject(ThemeService);

    protected readonly submenu = signal<'language' | 'theme' | null>(null);

    protected readonly profileId = computed(() => {
        return this.authService.userValue()?.id ?? '';
    });

    protected readonly profileEmail = computed(() => {
        return this.authService.userValue()?.email ?? '';
    });

    protected readonly modalMenu = new ModalModel();

    protected readonly language = this.uiOptions.value.culture;

    protected readonly languages = UILanguages.ALL;

    protected readonly profileUrl = this.apiUrlConfig.buildUrl('/identity-server/account/profile');

    protected readonly themeModes: ReadonlyArray<{ mode: ThemeMode; label: string }> = [
        { mode: 'light', label: 'common.themeLight' },
        { mode: 'dark', label: 'common.themeDark' },
        { mode: 'system', label: 'common.themeSystem' },
    ];

    protected changeLanguage(code: string) {
        Cookies.replace('.AspNetCore.Culture', `c=${code}|uic=${code}`, 365);

        location.reload();
    }

    protected changeTheme(mode: ThemeMode) {
        this.themeService.setMode(mode);
    }

    protected toggleProfile() {
        this.modalMenu.toggle();

        this.submenu.set(null);
    }

    protected toggleSubmenu(submenu: 'language' | 'theme') {
        this.submenu.update(x => x === submenu ? null : submenu);
    }

    protected logout() {
        this.authService.logoutRedirect('/');
    }
}
