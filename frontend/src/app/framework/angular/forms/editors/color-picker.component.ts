/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgIf } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, forwardRef, Input } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { ColorPickerModule } from 'ngx-color-picker';
import { MathHelper, ModalModel, StatefulControlComponent } from '@app/framework/internal';
import { ModalPlacementDirective } from '../../modals/modal-placement.directive';
import { ModalDirective } from '../../modals/modal.directive';
import { FocusComponent } from '../forms-helper';

export const SQX_COLOR_PICKER_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ColorPickerComponent), multi: true,
};

interface State {
    // The current value.
    value: string;

    // The foreground color.
    foreground: string;
}

@Component({
    standalone: true,
    selector: 'sqx-color-picker',
    styleUrls: ['./color-picker.component.scss'],
    templateUrl: './color-picker.component.html',
    providers: [
        SQX_COLOR_PICKER_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ColorPickerModule,
        FormsModule,
        ModalDirective,
        ModalPlacementDirective,
        NgIf,
    ],
})
export class ColorPickerComponent extends StatefulControlComponent<State, string> implements FocusComponent {
    private wasOpen = false;

    @Input()
    public placeholder = '';

    @Input()
    public mode: 'Input' | 'Circle' = 'Input';

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    public modal = new ModalModel();

    constructor() {
        super({ foreground: 'black', value: 'black' });

        this.modal.isOpenChanges.subscribe(open => {
            if (open) {
                this.wasOpen = true;
            } else {
                if (this.wasOpen) {
                    this.callTouched();
                }

                this.wasOpen = false;
            }
        });
    }

    public writeValue(value: any) {
        const previousColor = this.snapshot.value;

        if (previousColor !== value) {
            let foreground = 'black';

            if (MathHelper.toLuminance(MathHelper.parseColor(value)!) < 0.5) {
                foreground = 'white';
            }

            this.next({ value, foreground });
        }
    }

    public focus() {
        if (this.snapshot.isDisabled) {
            return;
        }

        this.modal.show();
    }

    public blur() {
        if (this.snapshot.isDisabled) {
            return;
        }

        this.callTouched();
    }

    public updateValue(value: string) {
        if (this.snapshot.isDisabled) {
            return;
        }

        if (this.snapshot.value !== value) {
            this.callChange(value);

            this.writeValue(value);
        }
    }
}
