/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, OnDestroy } from '@angular/core';
import { ControlValueAccessor } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';
import { onErrorResumeNext, skip } from 'rxjs/operators';

import { Types } from './../utils/types';

import { State } from './../state';

declare type UnsubscribeFunction = () => void;

export class ResourceOwner implements OnDestroy {
    private subscriptions: (Subscription | UnsubscribeFunction)[] = [];

    public own<T>(subscription: Subscription | UnsubscribeFunction | Observable<T>) {
        if (subscription) {
            if (Types.isFunction(subscription['subscribe'])) {
                const observable = <Observable<T>>subscription;

                this.subscriptions.push(observable.pipe(onErrorResumeNext()).subscribe());
            } else {
                this.subscriptions.push(<any>subscription);
            }
        }
    }

    public ngOnDestroy() {
        this.unsubscribeAll();
    }

    public unsubscribeAll() {
        try {
            for (let subscription of this.subscriptions) {
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

export abstract class StatefulComponent<T = any> extends State<T> implements OnDestroy {
    private readonly subscriptions = new ResourceOwner();
    private readonly subscription: Subscription;

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        state: T
    ) {
        super(state);

        this.subscription =
            this.changes.pipe(skip(1)).subscribe(() => {
                this.changeDetector.detectChanges();
            });
    }

    public ngOnDestroy() {
        this.subscription.unsubscribe();

        this.unsubscribeAll();
    }

    protected unsubscribeAll() {
        this.subscriptions.unsubscribeAll();
    }

    protected detach() {
        this.changeDetector.detach();
    }

    protected detectChanges() {
        this.changeDetector.detectChanges();
    }

    public own<R>(subscription: Subscription | UnsubscribeFunction | Observable<R>) {
        this.subscriptions.own(subscription);
    }
}

export abstract class StatefulControlComponent<T, TValue> extends StatefulComponent<T & { isDisabled: boolean }> implements ControlValueAccessor {
    private fnChanged = (v: any) => { /* NOOP */ };
    private fnTouched = () => { /* NOOP */ };

    constructor(changeDetector: ChangeDetectorRef, state: T) {
        super(changeDetector, { ...state, isDisabled: false });
    }

    public registerOnChange(fn: any) {
        this.fnChanged = fn;
    }

    public registerOnTouched(fn: any) {
        this.fnTouched = fn;
    }

    public callTouched() {
        this.fnTouched();
    }

    public callChange(value: TValue | null | undefined) {
        this.fnChanged(value);
    }

    public setDisabledState(isDisabled: boolean): void {
        this.next(s => ({ ...s, isDisabled }));
    }

    public abstract writeValue(obj: any): void;
}