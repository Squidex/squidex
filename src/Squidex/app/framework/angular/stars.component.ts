/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, forwardRef, Input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

const NOOP = () => { /* NOOP */ };

export const SQX_STARS_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => StarsComponent), multi: true
};

@Component({
    selector: 'sqx-stars',
    styleUrls: ['./stars.component.scss'],
    templateUrl: './stars.component.html',
    providers: [SQX_STARS_CONTROL_VALUE_ACCESSOR]
})
export class StarsComponent implements ControlValueAccessor {
    private changeCallback: (value: any) => void = NOOP;
    private touchedCallback: () => void = NOOP;
    private maximumStarsValue = 5;

    @Input()
    public set maximumStars(value: any) {
        value = value || 5;

        if (!(typeof value === 'number')) {
            value = 5;
        }

        if (this.maximumStarsValue !== value) {
            this.maximumStarsValue = value;

            this.starsArray = [];

            for (let i = 1; i <= value; i++) {
                this.starsArray.push(i);
            }
        }
    }

    public get maximumStars() {
        return this.maximumStarsValue;
    }

    public isDisabled = false;

    public stars: number;
    public starsArray: number[] = [1, 2, 3, 4, 5];

    public value: number | null = 1;

    public writeValue(value: any) {
        this.value = this.stars = value;
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;
    }

    public registerOnChange(fn: any) {
        this.changeCallback = fn;
    }

    public registerOnTouched(fn: any) {
        this.touchedCallback = fn;
    }

    public setPreview(value: number) {
        if (this.isDisabled) {
            return;
        }

        this.stars = value;
    }

    public stopPreview() {
        if (this.isDisabled) {
            return;
        }

        this.stars = this.value || 0;
    }

    public reset() {
        if (this.isDisabled) {
            return false;
        }

        if (this.value !== null) {
            this.value = null;
            this.stars = 0;

            this.changeCallback(this.value);
            this.touchedCallback();
        }

        return false;
    }

    public setValue(value: number) {
        if (this.isDisabled) {
            return false;
        }

        if (this.value !== value) {
            this.value = this.stars = value;

            this.changeCallback(this.value);
            this.touchedCallback();
        }

        return false;
    }
}