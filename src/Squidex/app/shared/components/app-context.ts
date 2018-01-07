/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable, OnDestroy } from '@angular/core';
import { Observable, Subscription } from 'rxjs';
import { ActivatedRoute } from '@angular/router';

import { MessageBus } from 'framework';

import {
    AppDto,
    AppsStoreService,
    AuthService,
    DialogService,
    ErrorDto,
    Notification,
    Profile
} from './../declarations-base';

@Injectable()
export class AppContext implements OnDestroy {
    private readonly appSubscription: Subscription;
    private appField: AppDto;

    public get app(): AppDto {
        return this.appField;
    }

    public get appChanges(): Observable<AppDto | null> {
        return this.appsStore.selectedApp;
    }

    public get appName(): string {
        return this.appField ? this.appField.name : '';
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
        public readonly appsStore: AppsStoreService,
        public readonly route: ActivatedRoute,
        public readonly bus: MessageBus
    ) {
        this.appSubscription =
            this.appsStore.selectedApp.take(1).subscribe(app => {
                this.appField = app!;
            });
    }

    public ngOnDestroy() {
        this.appSubscription.unsubscribe();
    }

    public confirmUnsavedChanges(): Observable<boolean> {
        return this.dialogs.confirmUnsavedChanges();
    }

    public notifyInfo(error: string) {
        this.dialogs.notify(Notification.info(error));
    }

    public notifyError(error: string | ErrorDto) {
        if (error instanceof ErrorDto) {
            this.dialogs.notify(Notification.error(error.displayMessage));
        } else {
            this.dialogs.notify(Notification.error(error));
        }
    }
}