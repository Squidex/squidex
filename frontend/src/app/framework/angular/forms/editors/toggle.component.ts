/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input, model } from '@angular/core';
import { FormValueControl } from '@angular/forms/signals';
import { Types } from '@app/framework/internal';

@Component({
    selector: 'sqx-toggle',
    styleUrls: ['./toggle.component.scss'],
    templateUrl: './toggle.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToggleComponent implements FormValueControl<boolean | null> {
    /** The value of the control. */
    public readonly value = model<boolean | null>(null);

    /** Disables the control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Indicates whether the control has been touched. */
    public readonly touched = model(false);

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
        this.touched.set(true);
    }
}
