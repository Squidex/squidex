/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs';

import {
    Log,
    User,
    UserManager
} from 'oidc-client';

import { ApiUrlConfig } from 'framework';

export class Profile {
    public get id(): string {
        return this.user.profile['sub'];
    }

    public get displayName(): string {
        return this.user.profile['urn:squidex:name'];
    }

    public get pictureUrl(): string {
        return this.user.profile['urn:squidex:picture'];
    }

    public get isAdmin(): boolean {
        return this.user.profile['role'] && this.user.profile['role'].toUpperCase() === 'ADMINISTRATOR';
    }

    public get isExpired(): boolean {
        return this.user.expired;
    }

    public get authToken(): string {
        return `${this.user.token_type} ${this.user.access_token}`;
    }

    public get token(): string {
        return `subject:${this.id}`;
    }

    constructor(
        public readonly user: User
    ) {
    }
}

@Injectable()
export class AuthService {
    private readonly userManager: UserManager;
    private readonly user$ = new ReplaySubject<Profile | null>();
    private currentUser: Profile = null;

    public get user(): Profile | null {
        return this.currentUser;
    }

    public get userChanges(): Observable<Profile | null> {
        return this.user$;
    }

    constructor(apiUrl: ApiUrlConfig) {
        if (!apiUrl) {
            return;
        }

        Log.logger = console;

        this.userManager = new UserManager({
                       client_id: 'squidex-frontend',
                           scope: 'squidex-api openid profile squidex-profile role',
                   response_type: 'id_token token',
                    redirect_uri: apiUrl.buildUrl('login;'),
        post_logout_redirect_uri: apiUrl.buildUrl('logout'),
             silent_redirect_uri: apiUrl.buildUrl('identity-server/client-callback-silent/'),
              popup_redirect_uri: apiUrl.buildUrl('identity-server/client-callback-popup/'),
                       authority: apiUrl.buildUrl('identity-server/'),
            automaticSilentRenew: true
        });

        this.userManager.events.addUserLoaded(user => {
            this.user$.next(new Profile(user));
        });

        this.userManager.events.addUserUnloaded(() => {
            this.user$.next(null);
        });

        this.user$.subscribe(user => {
            this.currentUser = user;
        });

        this.userManager.signinSilent();
    }

    public logoutRedirect(): Observable<any> {
        return Observable.fromPromise(this.userManager.signoutRedirect());
    }

    public logoutRedirectComplete(): Observable<any> {
        return Observable.fromPromise(this.userManager.signoutRedirectCallback());
    }

    public loginPopup(): Observable<any> {
        return Observable.fromPromise(this.userManager.signinPopup());
    }

    public loginRedirect(): Observable<any> {
        return Observable.fromPromise(this.userManager.signinRedirect());
    }

    public loginRedirectComplete(): Observable<any> {
        return Observable.fromPromise(this.userManager.signinRedirectCallback());
    }
}