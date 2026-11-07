/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, forwardRef, input, signal } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { SignalControlComponent, Types } from '@app/framework/internal';

export const SQX_TOGGLE_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ToggleComponent), multi: true,
};

@Component({
    selector: 'sqx-toggle',
    styleUrls: ['./toggle.component.scss'],
    templateUrl: './toggle.component.html',
    providers: [
        SQX_TOGGLE_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToggleComponent extends SignalControlComponent<boolean | null> {
    /** Allows null as third state, besides checked and unchecked. */
    public readonly threeStates = input(false, { transform: booleanAttribute });

    public readonly isChecked = signal<boolean | null>(null);

    public writeValue(obj: any) {
        this.isChecked.set(Types.isBoolean(obj) ? obj : null);
    }

    public changeState() {
        if (this.isDisabled()) {
            return;
        }

        let isChecked = this.isChecked();

        if (this.threeStates()) {
            if (isChecked) {
                isChecked = null;
            } else {
                isChecked = isChecked !== null;
            }
        } else {
            isChecked = !(isChecked === true);
        }

        this.isChecked.set(isChecked);

        this.callChange(isChecked);
        this.callTouched();
    }
}
