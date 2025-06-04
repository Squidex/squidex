/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, ElementRef, forwardRef, Input, ViewChild } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MathHelper, StatefulControlComponent } from '@app/framework/internal';
import { FocusComponent } from '../forms-helper';

export const SQX_COLOR_PICKER_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ColorPickerComponent), multi: true,
};

interface State {
    // The current value.
    value: string;

    // The hex color.
    parsed: string;

    // The foreground color.
    foreground: string;
}

@Component({
    selector: 'sqx-color-picker',
    styleUrls: ['./color-picker.component.scss'],
    templateUrl: './color-picker.component.html',
    providers: [
        SQX_COLOR_PICKER_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormsModule,
    ],
})
export class ColorPickerComponent extends StatefulControlComponent<State, string> implements FocusComponent {
    @Input()
    public placeholder = '';

    @Input()
    public mode: 'Input' | 'Circle' = 'Input';

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @ViewChild('input')
    public input?: ElementRef<HTMLInputElement>;

    constructor() {
        super({ foreground: 'black', value: 'black', parsed: '#000000' });
    }

    public writeValue(value: any) {
        const previousColor = this.snapshot.value;
        const parsedColor = parseColor(value);

        if (previousColor !== parsedColor) {
            let foreground = 'black';

            if (MathHelper.toLuminance(MathHelper.parseColor(parsedColor)!) < 0.5) {
                foreground = 'white';
            }

            this.next({ value, parsed: parsedColor, foreground });
        }
    }

    public focus() {
        if (this.snapshot.isDisabled) {
            return;
        }

        this.input?.nativeElement?.focus();
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

let canvas: HTMLCanvasElement;
function parseColor(color: string) {
    if (!canvas) {
        canvas = document.createElement('canvas');
    }

    const ctx = canvas.getContext('2d')!;
    if (!ctx) {
        return color;
    }

    ctx.fillStyle = color;
    return ctx.fillStyle;
}