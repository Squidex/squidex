/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, forwardRef, input, numberAttribute, signal } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { SignalControlComponent, Types } from '@app/framework/internal';
import { TranslatePipe } from '../../pipes/translate.pipe';

export const SQX_STARS_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => StarsComponent), multi: true,
};

@Component({
    selector: 'sqx-stars',
    styleUrls: ['./stars.component.scss'],
    templateUrl: './stars.component.html',
    providers: [
        SQX_STARS_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        TranslatePipe,
    ],
})
export class StarsComponent extends SignalControlComponent<number | null> {
    /** The number of stars to render. Shows a hint instead of stars when greater than 15. */
    public readonly maximumStars = input(5, { transform: (value: unknown) => numberAttribute(value, 5) });

    public readonly starsArray = computed(() => {
        const result: number[] = [];

        for (let i = 1; i <= this.maximumStars(); i++) {
            result.push(i);
        }

        return result;
    });

    public readonly value = signal<number | null>(null);
    public readonly valuePreview = signal(-1);

    public writeValue(obj: any) {
        const value = Types.isNumber(obj) ? obj : null;

        this.value.set(value);
        this.valuePreview.set(value || 0);
    }

    public setPreview(stars: number) {
        if (this.isDisabled()) {
            return;
        }

        this.valuePreview.set(stars);
    }

    public stopPreview() {
        if (this.isDisabled()) {
            return;
        }

        this.valuePreview.set(this.value() || 0);
    }

    public reset() {
        if (this.isDisabled()) {
            return false;
        }

        if (this.value()) {
            this.value.set(null);
            this.valuePreview.set(-1);

            this.callChange(null);
            this.callTouched();
        }

        return false;
    }

    public setValue(value: number) {
        if (this.isDisabled()) {
            return false;
        }

        if (this.value() !== value) {
            this.value.set(value);
            this.valuePreview.set(value);

            this.callChange(value);
            this.callTouched();
        }

        return false;
    }
}
