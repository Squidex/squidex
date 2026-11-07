/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Event, NavigationCancel, NavigationEnd, NavigationError, NavigationStart, Router } from '@angular/router';
import { Types } from '../utils/types';

@Injectable({
    providedIn: 'root',
})
export class LoadingService {
    private readonly loadingCount = signal(0);
    private readonly loadingOperations: { [key: string]: boolean } = {};

    public readonly loading = computed(() => {
        return this.loadingCount() > 0;
    });

    public scheduler: ((action: (() => void)) => void) = action => {
        setTimeout(action, 200);
    };

    constructor() {
        inject(Router).events.pipe(takeUntilDestroyed())
            .subscribe(event => {
                this.handleRouterEvent(event);
            });
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

    private handleRouterEvent(event: Event) {
        if (Types.is(event, NavigationStart)) {
            this.startLoading(event.id.toString());
            return;
        }

        if (Types.is(event, NavigationEnd) || Types.is(event, NavigationCancel) || Types.is(event, NavigationError)) {
            this.completeLoading(event.id.toString());
        }
    }
}
