/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Directive, inject, OnDestroy } from '@angular/core';
import { ControlValueAccessor } from '@angular/forms';
import { skip, Subscription } from 'rxjs';
import { State } from '../state';

@Directive()
export abstract class StatefulComponent<T extends {} = object> extends State<T> implements OnDestroy {
    private readonly subscription: Subscription;
    private readonly changeDetector = inject(ChangeDetectorRef);

    protected constructor(state: T) {
        super(state);

        this.subscription =
            this.changes.pipe(skip(1)).subscribe(() => {
                try {
                    this.changeDetector.detectChanges();
                } catch {
                    return;
                }
            });
    }

    public ngOnDestroy() {
        this.subscription.unsubscribe();
    }

    protected detach() {
        this.changeDetector.detach();
    }

    protected detectChanges() {
        this.changeDetector.detectChanges();
    }
}

type Disabled = { isDisabled: boolean };

export abstract class StatefulControlComponent<T extends {}, TValue> extends StatefulComponent<Disabled & T> implements ControlValueAccessor {
    private fnChanged = (_: any) => { /* NOOP */ };
    private fnTouched = () => { /* NOOP */ };

    protected constructor(state: T) {
        super({ ...state, isDisabled: false });
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
