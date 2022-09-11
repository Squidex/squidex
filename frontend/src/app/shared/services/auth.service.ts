/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Log, User, UserManager, WebStorageStateStore } from 'oidc-client';
import { concat, Observable, of, ReplaySubject, throwError, TimeoutError } from 'rxjs';
import { delay, mergeMap, retryWhen, take, timeout } from 'rxjs/operators';
import { ApiUrlConfig, Types } from '@app/framework';

export class Profile {
    public get id(): string {
        return this.user.profile['sub'];
    }

    public get email(): string {
        return this.user.profile['email']!;
    }

    public get displayName(): string {
        return this.user.profile['urn:squidex:name'];
    }

    public get pictureUrl(): string {
        return this.user.profile['urn:squidex:picture'];
    }

    public get notifoToken(): string | undefined {
        return this.user.profile['urn:squidex:notifo'];
    }

    public get isExpired(): boolean {
        return this.user.expired || false;
    }

    public get accessToken(): string {
        return this.user.access_token;
    }

    public get authorization(): string {
        return `${this.user!.token_type} ${this.user.access_token}`;
    }

    public get token(): string {
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

@Injectable()
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

        Log.logger = console;

        this.userManager = new UserManager({
                       client_id: 'squidex-frontend',
                           scope: 'squidex-api openid profile email permissions',
                   response_type: 'code',
                    redirect_uri: apiUrl.buildUrl('login;'),
        post_logout_redirect_uri: apiUrl.buildUrl('logout'),
             silent_redirect_uri: apiUrl.buildUrl('client-callback-silent.html'),
              popup_redirect_uri: apiUrl.buildUrl('client-callback-popup.html'),
                       authority: apiUrl.buildUrl('identity-server/'),
                       userStore: new WebStorageStateStore({ store: window.localStorage || window.sessionStorage }),
            automaticSilentRenew: true,
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

    public logoutRedirectComplete(): Observable<string | undefined> {
        return new Observable<string | undefined>(observer => {
            this.userManager.signoutRedirectCallback()
                .then(x => {
                    observer.next(x.state?.redirectPath);
                    observer.complete();
                }, err => {
                    observer.error(err);
                    observer.complete();
                });
        });
    }

    public loginPopup(redirectPath: string): Observable<string | undefined> {
        return new Observable<string | undefined>(observer => {
            this.userManager.signinPopup({ state: { redirectPath } })
                .then(x => {
                    observer.next(x.state?.redirectPath);
                    observer.complete();
                }, err => {
                    observer.error(err);
                    observer.complete();
                });
        });
    }

    public loginRedirectComplete(): Observable<string | undefined> {
        return new Observable<string | undefined>(observer => {
            this.userManager.signinRedirectCallback()
                .then(x => {
                    observer.next(x.state?.redirectPath);
                    observer.complete();
                }, err => {
                    observer.error(err);
                    observer.complete();
                });
        });
    }

    public loginSilent(): Observable<Profile> {
        const observable =
            new Observable<Profile>(observer => {
                this.userManager.signinSilent()
                    .then(x => {
                        observer.next(AuthService.createProfile(x));
                        observer.complete();
                    }, err => {
                        observer.error(err);
                        observer.complete();
                    });
            });

        return observable.pipe(
            timeout(2000),
            retryWhen(errors =>
                concat(
                    errors.pipe(
                        mergeMap(e => (Types.is(e, TimeoutError) ? of(e) : throwError(() => e))),
                        delay(500),
                        take(5)),
                    throwError(() => new Error('Retry limit exceeded.')),
                ),
            ),
        );
    }

    private static createProfile(user: User): Profile {
        return new Profile(user);
    }

    private checkState(promise: Promise<User | null>) {
        promise.then(user => {
            if (user) {
                this.user$.next(AuthService.createProfile(user));
            } else {
                this.user$.next(null);
            }

            return true;
        }, _ => {
            this.user$.next(null);

            return false;
        });
    }
}
