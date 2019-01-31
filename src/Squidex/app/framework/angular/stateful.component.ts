/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, OnDestroy, OnInit } from '@angular/core';
import { ControlValueAccessor } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';

import { Types } from './../utils/types';

import { State } from '../state';

declare type UnsubscribeFunction = () => void;

export class ResourceOwner implements OnDestroy {
    private subscriptions: (Subscription | UnsubscribeFunction)[] = [];

    public takeOver<T>(subscription: Subscription | UnsubscribeFunction | Observable<T>) {
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

export abstract class StatefulComponent<T = any> extends State<T> implements OnDestroy, OnInit {
    private readonly subscriptions = new ResourceOwner();

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        state: T
    ) {
        super(state);
    }

    public ngOnDestroy() {
        this.subscriptions.ngOnDestroy();
    }

    public ngOnInit() {
        this.changes.subscribe(() => {
            this.changeDetector.detectChanges();
        });
    }

    public takeOver<R>(subscription: Subscription | UnsubscribeFunction | Observable<R>) {
        this.subscriptions.takeOver(subscription);
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

export abstract class ExternalControlComponent<TValue> extends StatefulComponent<any> implements ControlValueAccessor {
    private fnChanged = (v: any) => { /* NOOP */ };
    private fnTouched = () => { /* NOOP */ };

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {});

        changeDetector.detach();
    }

    public registerOnChange(fn: any) {
        this.fnChanged = fn;
    }

    public registerOnTouched(fn: any) {
        this.fnTouched = fn;
    }

    protected callTouched() {
        this.fnTouched();
    }

    protected callChange(value: TValue) {
        this.fnChanged(value);
    }

    public abstract setDisabledState(isDisabled: boolean): void;

    public abstract writeValue(obj: any): void;
}