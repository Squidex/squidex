/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, inject, ViewRef } from '@angular/core';
import { catchError, EMPTY, Observable, Subscription } from 'rxjs';
import { Types } from '../utils/types';

export type UnsubscribeFunction = () => void;

export class Subscriptions {
    private subscriptions: (Subscription | UnsubscribeFunction)[] = [];

    constructor() {
        const viewRef = inject(ChangeDetectorRef) as ViewRef;

        viewRef.onDestroy(() => {
            this.unsubscribeAll();
        });
    }

    public add<T>(subscription: Subscription | UnsubscribeFunction | Observable<T> | null | undefined) {
        if (subscription) {
            if (Types.isFunction((subscription as any)['subscribe'])) {
                const observable = <Observable<T>>subscription;

                this.subscriptions.push(observable.pipe(catchError(_ => EMPTY)).subscribe());
            } else {
                this.subscriptions.push(<any>subscription);
            }
        }
    }

    public unsubscribeAll() {
        try {
            for (const subscription of this.subscriptions) {
                if (Types.isFunction(subscription)) {
                    subscription();
                } else {
                    subscription.unsubscribe();
                }
            }
        } finally {
            this.subscriptions = [];
        }
    }
}