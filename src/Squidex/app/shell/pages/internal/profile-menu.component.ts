/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { filter } from 'rxjs/operators';

import {
    ApiUrlConfig,
    AuthService,
    fadeAnimation,
    ModalModel,
    StatefulComponent
} from '@app/shared';

interface State {
    profileDisplayName: string;
    profileId: string;
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
        private readonly authService: AuthService
    ) {
        super(changeDetector, {
            profileDisplayName: '',
            profileId: '',
            profileUrl: apiUrl.buildUrl('/identity-server/account/profile')
        });
    }
    public ngOnInit() {
        this.own(
            this.authService.userChanges.pipe(filter(user => !!user))
                .subscribe(user => {
                    const profileId = user!.id;
                    const profileDisplayName = user!.displayName;

                    this.next(s => ({ ...s, profileId, profileDisplayName }));
                }));
    }

    public logout() {
        this.authService.logoutRedirect();
    }
}