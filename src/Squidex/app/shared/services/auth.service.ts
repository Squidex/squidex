/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';
import * as Ng2Http from '@angular/http';
import * as Ng2Router from '@angular/router';

import {
    Log,
    User,
    UserManager
} from 'oidc-client';

import { Observable, Subject } from 'rxjs';

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

    constructor(
        public readonly user: User
    ) {
    }
}

@Ng2.Injectable()
export class AuthService {
    private readonly userManager: UserManager;
    private readonly isAuthenticatedChanged$ = new Subject<boolean>();
    private loginCompleted = false;
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

    constructor(apiUrl: ApiUrlConfig,
        private readonly http: Ng2Http.Http,
        private readonly router: Ng2Router.Router,
    ) {
        if (!apiUrl) {
            return;
        }

        Log.logger = console;

        this.userManager = new UserManager({
                       client_id: 'squidex-frontend',
                           scope: 'squidex-api openid profile squidex-profile',
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
            this.loginCache =
                this.checkState(this.userManager.signinSilent())
                    .then(result => {
                        return result || this.checkState(this.userManager.signinSilent());
                    });

            return this.loginCache;
        }
    }

    public logout(): Observable<any> {
        return Observable.fromPromise(this.userManager.signoutRedirect());
    }

    public logoutComplete(): Observable<any> {
        return Observable.fromPromise(this.userManager.signoutRedirectCallback());
    }

    public login(): Observable<any> {
        return Observable.fromPromise(this.userManager.signinRedirect());
    }

    public loginComplete(): Observable<any> {
        return Observable.fromPromise(this.userManager.signinRedirectCallback());
    }

    public loginPopup(): Observable<boolean> {
        const promise =
            this.checkState(this.userManager.signinPopup())
                .then(result => {
                    return result || this.checkState(this.userManager.signinSilent());
                });

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

                    this.onAuthenticated(null);

                    return false;
                });

        return resultPromise;
    }

    public authGet(url: string, options?: Ng2Http.RequestOptions): Observable<Ng2Http.Response> {
        options = this.setRequestOptions(options);

        return this.checkResponse(this.http.get(url, options));
    }

    public authPut(url: string, data: any, options?: Ng2Http.RequestOptions): Observable<Ng2Http.Response> {
        options = this.setRequestOptions(options);

        return this.checkResponse(this.http.put(url, data, options));
    }

    public authDelete(url: string, options?: Ng2Http.RequestOptions): Observable<Ng2Http.Response> {
        options = this.setRequestOptions(options);
        
        return this.checkResponse(this.http.delete(url, options));
    }

    public authPost(url: string, data: any, options?: Ng2Http.RequestOptions): Observable<Ng2Http.Response> {
        options = this.setRequestOptions(options);

        return this.checkResponse(this.http.post(url, data, options));
    }

    private checkResponse(response: Observable<Ng2Http.Response>) {
        return response.catch((error: Ng2Http.Response) => {
            if (error.status === 401) {
                this.login();
            } else if (error.status === 403) {
                this.router.navigate(['/404']);
            }

            return Observable.throw(error);
        });
    }

    private setRequestOptions(options?: Ng2Http.RequestOptions) {
        if (!options) {
            options = new Ng2Http.RequestOptions();
        }

        if (!options.headers) {
            options.headers = new Ng2Http.Headers();
            options.headers.append('Content-Type', 'application/json');
        }

        if (this.currentUser && this.currentUser.user) {
            options.headers.append('Authorization', `${this.currentUser.user.token_type} ${this.currentUser.user.access_token}`);
        }

        return options;
    }
}