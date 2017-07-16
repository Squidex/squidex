/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';

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
    private readonly isAuthenticatedChanged$ = new Subject<boolean>();
    private loginCompleted: boolean | null = false;
    private loginCache: Promise<boolean> | null = null;
    private currentUser: Profile | null = null;

    private readonly isAuthenticatedChangedPublished$ =
        this.isAuthenticatedChanged$
            .distinctUntilChanged()
            .publishReplay(1);

    public get user(): Profile | null {
        return this.currentUser;
    }

    public get isAuthenticated(): Observable<boolean> {
        return this.isAuthenticatedChangedPublished$;
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
            this.onAuthenticated(user);
        });

        this.userManager.events.addUserUnloaded(() => {
            this.onDeauthenticated();
        });

        this.checkLogin();

        this.isAuthenticatedChangedPublished$.connect();
    }

    public checkLogin(): Promise<boolean> {
        if (this.loginCompleted) {
            return Promise.resolve(this.currentUser !== null);
        } else if (this.loginCache) {
            return this.loginCache;
        } else {
            this.loginCache = this.checkState(this.userManager.signinSilent());

            return this.loginCache;
        }
    }

    public logoutRedirect(): Observable<any> {
        return Observable.fromPromise(this.userManager.signoutRedirect());
    }

    public logoutRedirectComplete(): Observable<any> {
        return Observable.fromPromise(this.userManager.signoutRedirectCallback());
    }

    public loginRedirect(): Observable<any> {
        return Observable.fromPromise(this.userManager.signinRedirect());
    }

    public loginRedirectComplete(): Observable<any> {
        return Observable.fromPromise(this.userManager.signinRedirectCallback());
    }

    public loginPopup(): Observable<boolean> {
        const promise = this.checkState(this.userManager.signinPopup());

        return Observable.fromPromise(promise);
    }

    private onAuthenticated(user: User) {
        this.currentUser = new Profile(user);

        this.isAuthenticatedChanged$.next(true);
    }

    private onDeauthenticated() {
        this.currentUser = null;

        this.isAuthenticatedChanged$.next(false);
    }

    private checkState(promise: Promise<User>): Promise<boolean> {
        const resultPromise =
            promise
                .then(user => {
                    this.loginCache = null;
                    this.loginCompleted = null;

                    this.onAuthenticated(user);

                    return !!this.currentUser;
                }).catch((err) => {
                    this.loginCache = null;
                    this.loginCompleted = null;

                    this.onDeauthenticated();

                    return false;
                });

        return resultPromise;
    }
}