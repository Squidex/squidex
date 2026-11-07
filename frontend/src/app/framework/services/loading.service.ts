/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed, Injectable, OnDestroy, signal } from '@angular/core';
import { NavigationCancel, NavigationEnd, NavigationError, NavigationStart, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { Types } from '../utils/types';

@Injectable({
    providedIn: 'root',
})
export class LoadingService implements OnDestroy {
    private readonly routerSubscription: Subscription;
    private readonly loadingCount = signal(0);
    private readonly loadingOperations: { [key: string]: boolean } = {};

    public readonly loading = computed(() => {
        return this.loadingCount() > 0;
    });

    public scheduler: ((action: (() => void)) => void) = action => {
        setTimeout(action, 200);
    };

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

            this.loadingCount.update(x => x + 1);
        }
    }

    public completeLoading(key: string) {
        if (this.loadingOperations[key]) {
            delete this.loadingOperations[key];

            this.scheduler(() => {
                this.loadingCount.update(x => Math.max(0, x - 1));
            });
        }
    }
}
