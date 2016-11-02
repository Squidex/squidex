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

import { ApiUrlConfig } from './../../framework';

@Ng2.Injectable()
export class AuthService {
    private readonly userManager: UserManager;
    private readonly isAuthenticatedChanged$ = new BehaviorSubject<boolean>(false);
    private currentUser: User | null = null;
    private checkLoginPromise: Promise<boolean>;

    public get user(): User | null {
        return this.currentUser;
    }

    public get isAuthenticated(): boolean {
        return !!this.currentUser;
    }

    public get isAuthenticatedChanges(): Observable<boolean> {
        return this.isAuthenticatedChanged$;
    }

    constructor(apiUrl: ApiUrlConfig,
        private readonly http: Ng2Http.Http
    ) {
        Log.logger = console;

        if (apiUrl) {
            this.userManager = new UserManager({
                           client_id: 'squidex-frontend',
                               scope: 'squidex-api openid profile ',
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
        if (this.checkLoginPromise) {
            return this.checkLoginPromise;
        } else if (this.currentUser) {
            return Promise.resolve(true);
        } else {
            this.checkLoginPromise = 
                this.checkState(this.userManager.signinSilent())
                    .then(result => {
                        return result || this.checkState(this.userManager.signinSilent());
                    });

            return this.checkLoginPromise;
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

    private onAuthenticated(user: User) {
        this.currentUser = user;

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

        return this.http.get(url, options);
    }

    public authPut(url: string, data: any, options?: Ng2Http.RequestOptions): Observable<Ng2Http.Response> {
        options = this.setRequestOptions(options);

        return this.http.put(url, data, options);
    }

    public authDelete(url: string, options?: Ng2Http.RequestOptions): Observable<Ng2Http.Response> {
        options = this.setRequestOptions(options);
        return this.http.delete(url, options);
    }

    public authPost(url: string, data: any, options?: Ng2Http.RequestOptions): Observable<Ng2Http.Response> {
        options = this.setRequestOptions(options);

        return this.http.post(url, data, options);
    }

    private setRequestOptions(options?: Ng2Http.RequestOptions) {
        if (!options) {
            options = new Ng2Http.RequestOptions();
        }

        if (!options.headers) {
            options.headers = new Ng2Http.Headers();
            options.headers.append('Content-Type', 'application/json');
        }
        options.headers.append('Authorization', `${this.currentUser.token_type} ${this.currentUser.access_token}`);

        return options;
    }
}