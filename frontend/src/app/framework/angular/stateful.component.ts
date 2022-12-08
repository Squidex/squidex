/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Directive, OnDestroy } from '@angular/core';
import { ControlValueAccessor } from '@angular/forms';
import { catchError, EMPTY, Observable, skip, Subscription } from 'rxjs';
import { State } from './../state';
import { Types } from './../utils/types';

declare type UnsubscribeFunction = () => void;

@Directive()
export class ResourceOwner implements OnDestroy {
    private subscriptions: (Subscription | UnsubscribeFunction)[] = [];

    public own<T>(subscription: Subscription | UnsubscribeFunction | Observable<T> | null | undefined) {
        if (subscription) {
            if (Types.isFunction(subscription['subscribe'])) {
                const observable = <Observable<T>>subscription;

                this.subscriptions.push(observable.pipe(catchError(_ => EMPTY)).subscribe());
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

@Directive()
export abstract class StatefulComponent<T extends {} = object> extends State<T> implements OnDestroy {
    private readonly subscriptions = new ResourceOwner();
    private readonly subscription: Subscription;

    protected constructor(
        private readonly changeDetector: ChangeDetectorRef,
        state: T,
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

    public own<R>(subscription: Subscription | UnsubscribeFunction | Observable<R> | null | undefined) {
        this.subscriptions.own(subscription);
    }
}

type Disabled = { isDisabled: boolean };

export abstract class StatefulControlComponent<T extends {}, TValue> extends StatefulComponent<Disabled & T> implements ControlValueAccessor {
    private fnChanged = (_: any) => { /* NOOP */ };
    private fnTouched = () => { /* NOOP */ };

    protected constructor(changeDetector: ChangeDetectorRef, state: T) {
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

    public callChange(value: TValue | undefined | null) {
        this.fnChanged(value);
    }

    public setDisabledState(isDisabled: boolean) {
        this.next({ isDisabled } as any);

        this.onDisabled(this.snapshot.isDisabled);
    }

    public onDisabled(_isDisabled: boolean) {
        /* NOOP */
    }

    public abstract writeValue(obj: any): void;
}
