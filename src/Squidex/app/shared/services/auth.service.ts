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

import { BehaviorSubject, Observable } from 'rxjs';

import { ApiUrlConfig } from 'framework';

export class Profile {
    public get displayName() {
        return this.user.profile['urn:squidex:name'];
    }

    public get pictureUrl() {
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
    private readonly isAuthenticatedChanged$ = new BehaviorSubject<boolean>(false);
    private currentUser: Profile | null = null;

    public get user(): Profile | null {
        return this.currentUser;
    }

    public get isAuthenticated(): boolean {
        return !!this.currentUser;
    }

    public get isAuthenticatedChanges(): Observable<boolean> {
        return this.isAuthenticatedChanged$;
    }

    constructor(apiUrl: ApiUrlConfig,
        private readonly http: Ng2Http.Http,
        private readonly router: Ng2Router.Router,
    ) {
        Log.logger = console;

        if (apiUrl) {
            this.userManager = new UserManager({
                           client_id: 'squidex-frontend',
                               scope: 'squidex-api openid profile squidex-profile',
                       response_type: 'id_token token',
                        redirect_uri: apiUrl.buildUrl('/login;'),
            post_logout_redirect_uri: apiUrl.buildUrl('/logout;'),
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
        }
    }

    public checkLogin(): Promise<boolean> {
        if (this.currentUser) {
            return Promise.resolve(true);
        } else {
            const promise = 
                this.checkState(this.userManager.signinSilent())
                    .then(result => {
                        return result || this.checkState(this.userManager.signinSilent());
                    });

            return promise;
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
        this.isAuthenticatedChanged$.next(true);

        this.currentUser = new Profile(user);
    }

    private onDeauthenticated() {
        this.isAuthenticatedChanged$.next(false);

        this.currentUser = null;
    }

    private checkState(promise: Promise<User>): Promise<boolean> {
        const resultPromise =
            promise
                .then(user => {
                    if (user) {
                        this.onAuthenticated(user);
                    }
                    return !!this.currentUser;
                }).catch((err) => {
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
        options.headers.append('Authorization', `${this.currentUser.user.token_type} ${this.currentUser.user.access_token}`);

        return options;
    }
}