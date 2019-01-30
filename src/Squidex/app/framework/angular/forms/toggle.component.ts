/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, forwardRef, Input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

import { Types } from '@app/framework/internal';

import { StatefulComponent } from '../stateful.component';

export const SQX_TOGGLE_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ToggleComponent), multi: true
};

interface State {
    isDisabled: boolean;
    isChecked: boolean | null;
}

@Component({
    selector: 'sqx-toggle',
    styleUrls: ['./toggle.component.scss'],
    templateUrl: './toggle.component.html',
    providers: [SQX_TOGGLE_CONTROL_VALUE_ACCESSOR]
})
export class ToggleComponent extends StatefulComponent<State> implements ControlValueAccessor {
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };

    @Input()
    public threeStates = false;

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            isChecked: null,
            isDisabled: false
        });
    }

    public writeValue(obj: any) {
        this.next({ isChecked: Types.isBoolean(obj) ? obj : null });
    }

    public setDisabledState(isDisabled: boolean): void {
        this.next({ isDisabled });
    }

    public registerOnChange(fn: any) {
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
    }

    public changeState(event: MouseEvent) {
        let { isDisabled, isChecked } = this.snapshot;

        if (isDisabled) {
            return;
        }

        if (this.threeStates && (event.ctrlKey || event.shiftKey)) {
            if (isChecked) {
                isChecked = null;
            } else if (isChecked === null) {
                isChecked = false;
            } else {
                isChecked = true;
            }
        } else {
            isChecked = !(isChecked === true);
        }

        this.next({ isChecked });

        this.callChange(isChecked);
        this.callTouched();
    }
}