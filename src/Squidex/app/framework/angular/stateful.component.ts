/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, OnDestroy, OnInit } from '@angular/core';
import { ControlValueAccessor } from '@angular/forms';
import { Subscription } from 'rxjs';

import { Types } from './../utils/types';

import { State } from '../state';

declare type UnsubscribeFunction = () => void;

export abstract class StatefulComponent<T = any> extends State<T> implements OnDestroy, OnInit {
    private subscriptions: (Subscription | UnsubscribeFunction)[] = [];

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        state: T
    ) {
        super(state);
    }

    protected observe(subscription: Subscription | UnsubscribeFunction) {
        if (subscription) {
            this.subscriptions.push(subscription);
        }
    }

    public ngOnInit() {
        this.changes.subscribe(() => {
            this.changeDetector.detectChanges();
        });
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