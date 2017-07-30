/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { Observable, Observer, ReplaySubject, TimeoutError } from 'rxjs';

import {
    Log,
    User,
    UserManager,
    WebStorageStateStore
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
    private readonly user$ = new ReplaySubject<Profile | null>(1);
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
                       userStore: new WebStorageStateStore({ store: window.localStorage || window.sessionStorage }),
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

        this.checkState(this.userManager.getUser());
    }

    public logoutRedirect() {
       this.userManager.signoutRedirect();
    }

    public loginRedirect() {
        this.userManager.signinRedirect();
    }

    public logoutRedirectComplete(): Observable<any> {
        return Observable.create((observer: Observer<any>) => {
            this.userManager.signoutRedirectCallback()
                .then(x => {
                    observer.next(x);
                    observer.complete();
                }, err => {
                    observer.error(err);
                    observer.complete();
                });
        });
    }

    public loginPopup(): Observable<Profile> {
        return Observable.create((observer: Observer<Profile>) => {
            this.userManager.signinPopup()
                .then(x => {
                    observer.next(this.createProfile(x));
                    observer.complete();
                }, err => {
                    observer.error(err);
                    observer.complete();
                });
        });
    }

    public loginRedirectComplete(): Observable<Profile> {
        return Observable.create((observer: Observer<Profile>) => {
            this.userManager.signinRedirectCallback()
                .then(x => {
                    observer.next(this.createProfile(x));
                    observer.complete();
                }, err => {
                    observer.error(err);
                    observer.complete();
                });
        });
    }

    public loginSilent(): Observable<Profile> {
        const observable: Observable<Profile> =
            Observable.create((observer: Observer<Profile>) => {
                this.userManager.signinSilent()
                    .then(x => {
                        observer.next(this.createProfile(x));
                        observer.complete();
                    }, err => {
                        observer.error(err);
                        observer.complete();
                    });
            });

        return observable.timeout(1000).retryWhen(errors => errors.filter(e => e instanceof TimeoutError));
    }

    private createProfile(user: User) {
        return user ? new Profile(user) : null;
    }

    private checkState(promise: Promise<User>) {
        promise.then(user => {
            this.user$.next(this.createProfile(user));

            return true;
        }, err => {
            this.user$.next(null);

            return false;
        });
    }
}