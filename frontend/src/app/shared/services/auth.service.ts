/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Log, User, UserManager, WebStorageStateStore } from 'oidc-client-ts';
import { Observable, ReplaySubject } from 'rxjs';
import { ApiUrlConfig } from '@app/framework';

export class Profile {
    public get id() {
        return this.user.profile['sub'];
    }

    public get email() {
        return this.user.profile['email']!;
    }

    public get displayName() {
        return this.user.profile['urn:squidex:name'] as string;
    }

    public get pictureUrl() {
        return this.user.profile['urn:squidex:picture'];
    }

    public get notifoToken(): string | undefined {
        return this.user.profile['urn:squidex:notifo'] as string | undefined;
    }

    public get isExpired() {
        return this.user.expired || false;
    }

    public get accessToken() {
        return this.user.access_token;
    }

    public get authorization() {
        return `${this.user!.token_type} ${this.user.access_token}`;
    }

    public get token() {
        return `subject:${this.id}`;
    }

    constructor(
        public readonly user: User,
    ) {
    }

    public export() {
        const result: any = {
            id: this.id,
            accessToken: this.accessToken,
            accessCode: this.accessToken,
            authorization: this.authorization,
            displayName: this.displayName,
            email: this.email,
            notifoEnabled: !!this.notifoToken,
            notifoToken: this.notifoToken,
            picture: this.pictureUrl,
            pictureUrl: this.pictureUrl,
            token: this.token,
            user: this.user,
        };

        Object.assign(result, this.user.profile);

        return result;
    }
}

@Injectable({
    providedIn: 'root',
})
export class AuthService {
    private readonly userManager!: UserManager;
    private readonly user$ = new ReplaySubject<Profile | null>(1);
    private currentUser: Profile | null = null;

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

        Log.setLogger(console);

        this.userManager = new UserManager({
                       client_id: 'squidex-frontend',
                           scope: 'squidex-api openid profile email permissions',
                    redirect_uri: apiUrl.buildUrl('login;'),
        post_logout_redirect_uri: apiUrl.buildUrl('logout'),
             silent_redirect_uri: apiUrl.buildUrl('client-callback-silent.html'),
              popup_redirect_uri: apiUrl.buildUrl('client-callback-popup.html'),
                       authority: apiUrl.buildUrl('identity-server/'),
                      userStore: new WebStorageStateStore({ store: window.localStorage || window.sessionStorage }),
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

        this.checkState(this.userManager.getUser());
    }

    public logoutRedirect(redirectPath: string) {
        this.userManager.signoutRedirect({ state: { redirectPath } });
    }

    public loginRedirect(redirectPath: string) {
        this.userManager.signinRedirect({ state: { redirectPath } });
    }

    public async logoutRedirectComplete() {
        const result = await this.userManager.signoutRedirectCallback();

        return getRedirectPath(result.userState);
    }

    public async loginPopup(redirectPath: string) {
        const result = await this.userManager.signinPopup({ state: { redirectPath } });

        return getRedirectPath(result.state);
    }

    public async loginRedirectComplete() {
        const result = await this.userManager.signinRedirectCallback();

        return getRedirectPath(result.state);
    }

    public async loginSilent() {
        const MAX_ATTEMPTS = 5;

        for (let attempt = 1; attempt <= MAX_ATTEMPTS; attempt++) {
            const promiseTimeout = delayPromise(2000);
            const promiseUser = this.userManager.signinRedirectCallback();

            const result = Promise.race([promiseTimeout, promiseUser]);

            if (result === promiseUser) {
                return getProfile(await promiseUser);
            }

            if (attempt < MAX_ATTEMPTS) {
                await delayPromise(500);
            }
        }

        throw new Error('Retry limit exceeded.');
    }

    private async checkState(promise: Promise<User | null>) {
        try {
            const user = await promise;

            if (user) {
                this.user$.next(getProfile(user));
            } else {
                this.user$.next(null);
            }

            return true;
        } catch {
            this.user$.next(null);

            return false;
        }
    }
}

function getProfile(user: User): Profile {
    return new Profile(user);
}

function getRedirectPath(state: any) {
    return state?.['redirectPath'] as string | undefined;
}

function delayPromise(ms: number) {
    return new Promise(resolve => {
        setTimeout(resolve, ms);
    });
}