/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, computed, Directive, input, signal } from '@angular/core';
import { ControlValueAccessor } from '@angular/forms';

@Directive()
export abstract class SignalControlComponent<TValue> implements ControlValueAccessor {
    private readonly isDisabledByForm = signal(false);
    private fnChanged = (_: any) => { /* NOOP */ };
    private fnTouched = () => { /* NOOP */ };

    /** Disables the control, in addition to the disabled state of the form control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    public readonly isDisabled = computed(() => {
        return this.disabled() || this.isDisabledByForm();
    });

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
        this.isDisabledByForm.set(isDisabled);
    }

    public abstract writeValue(obj: any): void;
}