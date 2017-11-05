/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ActivatedRoute } from '@angular/router';

import { MessageBus } from 'framework';

import {
    AppDto,
    AuthService,
    DialogService,
    ErrorDto,
    Notification,
    Profile
} from './../declarations-base';

@Injectable()
export class AppContext {
    private appField: AppDto;

    public get app(): AppDto {
        return this.appField;
    }

    public get appName(): string {
        return this.appField.name;
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
        public readonly route: ActivatedRoute,
        public readonly bus: MessageBus
    ) {
        Observable.merge(...this.route.pathFromRoot.map(r => r.data)).map(d => d.app).filter(a => !!a)
            .subscribe((app: AppDto) => {
                this.appField = app;
            });
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