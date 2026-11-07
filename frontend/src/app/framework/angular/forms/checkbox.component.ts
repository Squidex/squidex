/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input, model } from '@angular/core';
import { Types } from '@app/framework/internal';
import { FormValueControlBase } from './form-value-control';

@Component({
    selector: 'sqx-checkbox',
    template: `
        <input
            class="form-check-input"
            [checked]="isChecked()"
            [disabled]="disabled()"
            [id]="inputId()"
            [indeterminate]="isIndeterminate()"
            (blur)="markTouched()"
            (click)="toggle($event)"
            type="checkbox" />
    `,
    host: {
        'style': 'display: contents',
    },
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CheckboxComponent extends FormValueControlBase<boolean | null> {
    /** The value of the control. */
    public readonly value = model<boolean | null>(null);

    /** The ID of the inner checkbox, used by labels. */
    public readonly inputId = input<string | null>(null);

    /** Cycles through checked, unchecked and indeterminate on click instead of only checked and unchecked. */
    public readonly threeStates = input(true, { transform: booleanAttribute });

    protected readonly isChecked = computed(() => {
        return this.value() === true;
    });

    protected readonly isIndeterminate = computed(() => {
        return !Types.isBoolean(this.value());
    });

    protected toggle(event: Event) {
        // The visual state is always derived from the value.
        event.preventDefault();

        const current = this.value();
        if (!this.threeStates()) {
            this.value.set(current !== true);
            return;
        }

        if (current) {
            this.value.set(null);
        } else {
            this.value.set(current !== null);
        }
    }
}
