/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, computed, input, model } from '@angular/core';
import { Types } from '@app/framework/internal';
import { FormValueControlBase } from '../form-value-control';

@Component({
    selector: 'sqx-toggle',
    styleUrls: ['./toggle.component.scss'],
    templateUrl: './toggle.component.html',
})
export class ToggleComponent extends FormValueControlBase<boolean | null> {
    /** The value of the control. */
    public readonly value = model<boolean | null>(null);

    /** Allows null as third state, besides checked and unchecked. */
    public readonly threeStates = input(false, { transform: booleanAttribute });

    protected readonly isChecked = computed(() => {
        const value = this.value();

        return Types.isBoolean(value) ? value : null;
    });

    protected changeState() {
        if (this.disabled()) {
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

        this.value.set(isChecked);
        this.markTouched();
    }
}
