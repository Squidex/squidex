/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ActivatedRoute } from '@angular/router';

import { MessageBus } from '@app/framework';

import {
    AppDto,
    AppsState,
    AuthService,
    DialogService,
    ErrorDto,
    Profile
} from '@app/shared/internal';

@Injectable()
export class AppContext {
    public get app(): AppDto {
        return this.appsState.snapshot.selectedApp!;
    }

    public get appChanges(): Observable<AppDto | null> {
        return this.appsState.selectedApp;
    }

    public get appName(): string {
        return this.app.name;
    }

    public get userToken(): string {
        return this.authService.user!.token;
    }

    public get userId(): string {
        return this.authService.user!.id;
    }

    public get user(): Profile {
        return this.authService.user!;
    }

    constructor(
        public readonly dialogs: DialogService,
        public readonly authService: AuthService,
        public readonly appsState: AppsState,
        public readonly route: ActivatedRoute,
        public readonly bus: MessageBus
    ) {
    }

    public confirmUnsavedChanges(): Observable<boolean> {
        return this.dialogs.confirmUnsavedChanges();
    }

    public notifyInfo(error: string) {
        this.dialogs.notifyInfo(error);
    }

    public notifyError(error: string | ErrorDto) {
        this.dialogs.notifyError(error);
    }
}