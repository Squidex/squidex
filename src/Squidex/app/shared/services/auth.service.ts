/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { 
    Log, 
    User,
    UserManager 
} from 'oidc-client';

import { Observable } from 'rxjs';

import { ApiUrlConfig } from './../../framework';

@Ng2.Injectable()
export class AuthService {
    private readonly userManager: UserManager;
    private currentUser: User | null = null;

    public get user(): User | null {
        return this.currentUser;
    }

    public get isAuthenticated() {
        return this.currentUser;
    }

    constructor(apiUrl: ApiUrlConfig) {
        Log.logger = console;

        this.userManager = new UserManager({
                      client_id: 'management-portal', 
            silent_redirect_uri: apiUrl.buildUrl('account/client-silent'), 
             popup_redirect_uri: apiUrl.buildUrl('account/client-popup'),
                      authority: apiUrl.buildUrl('/'), 
        });

        this.userManager.getUser()
            .then((user) => {
                debugger;
                this.currentUser = user;
            })
            .catch((err) => {
                this.currentUser = null;
            });

        this.userManager.events.addUserUnloaded(() => {
                this.currentUser = null;
        });
    }

    public login(): Observable<User> {
        let userPromise = 
            this.userManager.signinSilent()
                .then(user => {
                    if (user) {
                        return user;
                    } else {
                        return this.userManager.signinPopup();
                    }
                })
                .catch(() => this.userManager.signinPopup());

        return Observable.fromPromise(userPromise);
    }

    public logout(): Observable<any> {
        return Observable.fromPromise(this.userManager.signoutRedirectCallback());
    }
}