/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, forwardRef, Input, numberAttribute } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { StatefulControlComponent, Types } from '@app/framework/internal';
import { TranslatePipe } from '../../pipes/translate.pipe';

export const SQX_STARS_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => StarsComponent), multi: true,
};

interface State {
    // The current stars to show.
    stars: number;

    // The array for rendering the stars.
    starsArray: ReadonlyArray<number>;

    // The selected value.
    value: number | null;
}

@Component({
    standalone: true,
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
export class StarsComponent extends StatefulControlComponent<State, number | null> {
    private maximumStarsValue = 5;

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @Input({ transform: numberAttribute })
    public set maximumStars(value: number) {
        const maxStars: number = Types.isNumber(value) ? value : 5;

        if (this.maximumStarsValue !== maxStars) {
            this.maximumStarsValue = value;

            const starsArray: number[] = [];

            for (let i = 1; i <= maxStars; i++) {
                starsArray.push(i);
            }

            this.next({ starsArray });
        }
    }

    public get maximumStars() {
        return this.maximumStarsValue;
    }

    constructor() {
        super({
            stars: -1,
            starsArray: [1, 2, 3, 4, 5],
            value: 1,
        });
    }

    public writeValue(obj: any) {
        const stars = Types.isNumber(obj) ? obj : 0;

        this.next({ stars });
    }

    public setPreview(stars: number) {
        if (this.snapshot.isDisabled) {
            return;
        }

        this.next({ stars });
    }

    public stopPreview() {
        if (this.snapshot.isDisabled) {
            return;
        }

        this.next(s => ({ ...s, stars: s.value || 0 }));
    }

    public reset() {
        if (this.snapshot.isDisabled) {
            return false;
        }

        if (this.snapshot.value) {
            this.next({ stars: -1, value: null });

            this.callChange(null);
            this.callTouched();
        }

        return false;
    }

    public setValue(value: number) {
        if (this.snapshot.isDisabled) {
            return false;
        }

        if (this.snapshot.value !== value) {
            this.next({ stars: value, value });

            this.callChange(value);
            this.callTouched();
        }

        return false;
    }
}
