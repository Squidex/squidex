/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input, linkedSignal, model, numberAttribute } from '@angular/core';
import { FormValueControl } from '@angular/forms/signals';
import { Types } from '@app/framework/internal';
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
    selector: 'sqx-stars',
    styleUrls: ['./stars.component.scss'],
    templateUrl: './stars.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        TranslatePipe,
    ],
})
export class StarsComponent implements FormValueControl<number | null> {
    /** The number of selected stars. */
    public readonly value = model<number | null>(null);

    /** Disables the control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Indicates whether the control has been touched. */
    public readonly touched = model(false);

    /** The number of stars to render. Shows a hint instead of stars when greater than 15. */
    public readonly maximumStars = input(5, { transform: (value: unknown) => numberAttribute(value, 5) });

    protected readonly valuePreview = linkedSignal(() => this.valueNumber() || 0);

    protected readonly starsArray = computed(() => {
        const result: number[] = [];

        for (let i = 1; i <= this.maximumStars(); i++) {
            result.push(i);
        }

        return result;
    });

    protected readonly valueNumber = computed(() => {
        const value = this.value();

        return Types.isNumber(value) ? value : null;
    });

    protected setPreview(stars: number) {
        if (this.disabled()) {
            return;
        }

        this.valuePreview.set(stars);
    }

    protected stopPreview() {
        if (this.disabled()) {
            return;
        }

        this.valuePreview.set(this.valueNumber() || 0);
    }

    protected reset() {
        if (this.disabled()) {
            return false;
        }

        if (this.valueNumber()) {
            this.value.set(null);
            this.touched.set(true);
        }

        return false;
    }

    protected setValue(value: number) {
        if (this.disabled()) {
            return false;
        }

        if (this.valueNumber() !== value) {
            this.value.set(value);
            this.touched.set(true);
        }

        return false;
    }
}
