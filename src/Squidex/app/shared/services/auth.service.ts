/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable, Observer, of, ReplaySubject, throwError, TimeoutError } from 'rxjs';
import { concat, delay, mergeMap, retryWhen, take, timeout } from 'rxjs/operators';

import {
    Log,
    User,
    UserManager,
    WebStorageStateStore
} from 'oidc-client';

import {
    ApiUrlConfig,
    Permission,
    Types
} from '@app/framework';

export class Profile {
    public readonly permissions: Permission[];

    public get id(): string {
        return this.user.profile['sub'];
    }

    public get email(): string {
        return this.user.profile['email'];
    }

    public get displayName(): string {
        return this.getUserName();
    }

    public get pictureUrl(): string {
        return this.user.profile['urn:squidex:picture'];
    }

    public get isExpired(): boolean {
        return this.user.expired || false;
    }

    public get authToken(): string {
        return `${this.user!.token_type} ${this.user.access_token}`;
    }

    public get token(): string {
        return `subject:${this.id}`;
    }

    private decodedToken: any;

    constructor(
        public readonly user: User
    ) {
        this.parseJwt(this.user.access_token);
        const permissions = this.getPermission();

        if (Types.isArrayOfString(permissions)) {
            this.permissions = permissions.map(x => new Permission(x));
        } else if (Types.isString(permissions)) {
            this.permissions = [new Permission(permissions)];
        } else {
            this.permissions = [];
        }

    }

    private getPermission(): string[] {
        const group = this.decodedToken['group'];

        const adminUserGroup = ['ICIS_Darwin', 'ICIS_AppSupport', 'RBI-QHS-SECURITY-ICIS-APPLICATIONS-SUPPORT'];
        const adminPermissions = ['squidex.*', 'squidex.admin.*', 'squidex.apps.commentary.*'];

        if (Types.isArrayOfString(group) && group.some(item => adminUserGroup.includes(item))) {
            return adminPermissions;
        } else {
            return this.user.profile['urn:squidex:permissions'];
        }
    }

    private getUserName(): string {
        return this.decodedToken['given_name'];
    }

    private parseJwt(token: string): any {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        this.decodedToken = JSON.parse(window.atob(base64));
    }
}

@Injectable()
export class AuthService {
    private readonly userManager: UserManager;
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

        Log.logger = console;

        this.userManager = new UserManager({
            client_id: 'vega.cms',
            scope: 'openid all',
            response_type: 'id_token token',
            redirect_uri: apiUrl.buildUrl('login'),
            post_logout_redirect_uri: apiUrl.buildUrl('logout'),
            silent_redirect_uri: apiUrl.buildUrl('client-callback-silent'),
            popup_redirect_uri: apiUrl.buildUrl('client-callback-popup'),
            authority: 'https://identityservice.systest.tesla.cha.rbxd.ds/',
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
            Observable.create((observer: Observer<Profile | null>) => {
                this.userManager.signinSilent()
                    .then(x => {
                        observer.next(this.createProfile(x));
                        observer.complete();
                    }, err => {
                        observer.error(err);
                        observer.complete();
                    });
            });

        return observable.pipe(
            timeout(2000),
            retryWhen(errors => errors.pipe(
                mergeMap(e => Types.is(e, TimeoutError) ? of(e) : throwError(e)),
                delay(500),
                take(5),
                concat(throwError(new Error('Retry limit exceeded.'))))));
    }

    private createProfile(user: User): Profile {
        return new Profile(user);
    }

    private checkState(promise: Promise<User | null>) {
        promise.then(user => {
            if (user) {
                this.user$.next(this.createProfile(user));
            } else {
                this.user$.next(null);
            }

            return true;
        }, error => {
            this.user$.next(null);

            return false;
        });
    }
}