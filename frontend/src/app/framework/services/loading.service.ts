/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable, OnDestroy } from '@angular/core';
import { NavigationCancel, NavigationEnd, NavigationError, NavigationStart, Router } from '@angular/router';
import { BehaviorSubject, map, Observable, Subscription } from 'rxjs';
import { Types } from './../utils/types';

@Injectable()
export class LoadingService implements OnDestroy {
    private readonly routerSubscription: Subscription;
    private readonly loading$ = new BehaviorSubject(0);
    private readonly loadingOperations: { [key: string]: boolean } = {};

    public get loading(): Observable<boolean> {
        return this.loading$.pipe(map(x => x > 0));
    }

    constructor(router: Router) {
        this.routerSubscription =
            router.events.subscribe(event => {
                if (Types.is(event, NavigationStart)) {
                    this.startLoading(event.id.toString());
                } else if (
                    Types.is(event, NavigationEnd) ||
                    Types.is(event, NavigationCancel) ||
                    Types.is(event, NavigationError)) {
                    this.completeLoading(event.id.toString());
                }
            });
    }

    public ngOnDestroy() {
        if (this.routerSubscription) {
            this.routerSubscription.unsubscribe();
        }
    }

    public startLoading(key: string) {
        if (!this.loadingOperations[key]) {
            this.loadingOperations[key] = true;

            this.loading$.next(this.loading$.value + 1);
        }
    }

    public completeLoading(key: string) {
        if (this.loadingOperations[key]) {
            delete this.loadingOperations[key];

            setTimeout(() => {
                const value = this.loading$.value;

                if (value > 0) {
                    this.loading$.next(value - 1);
                }
            }, 250);
        }
    }
}
