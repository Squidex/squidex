/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Directive, effect, input, ModelSignal, output, untracked, WritableSignal } from '@angular/core';
import { FormValueControl } from '@angular/forms/signals';
import { Types } from '@app/framework/internal';

@Directive()
export abstract class FormValueControlBase<T> implements FormValueControl<T> {
    /** Disables the control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Indicates whether the value is invalid, provided by the form. */
    public readonly invalid = input(false, { transform: booleanAttribute });

    /** Indicates whether the control has been touched, provided by the form. */
    public readonly touched = input(false, { transform: booleanAttribute });

    /** Emits when the user has finished interacting with the control. */
    public readonly touch = output();

    public abstract readonly value: ModelSignal<T>;

    public markTouched() {
        this.touch.emit();
    }
}

const UNSET = {};

// The value is provided by a function, so that the helper can be used in field initializers before the value model exists.
export function syncExternalValue<T>(value: () => WritableSignal<T>, onExternalChange: (value: T) => void) {
    // The value the control has written itself, which must not reset the internal state.
    let lastValue: T | typeof UNSET = UNSET;

    effect(() => {
        const current = value()();
        if (lastValue !== UNSET && Types.equals(current, lastValue)) {
            return;
        }

        lastValue = current;
        untracked(() => onExternalChange(current));
    });

    return (newValue: T) => {
        lastValue = newValue;
        value().set(newValue);
    };
}
