/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, ElementRef, forwardRef, input, signal, viewChild } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MathHelper, SignalControlComponent } from '@app/framework/internal';
import { FocusComponent } from '../forms-helper';

export const SQX_COLOR_PICKER_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ColorPickerComponent), multi: true,
};

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
export class ColorPickerComponent extends SignalControlComponent<string> implements FocusComponent {
    private readonly inputElement = viewChild<ElementRef<HTMLElement>>('input');

    /** The placeholder of the text input. */
    public readonly placeholder = input('');

    /** Renders a text input or only a colored circle. */
    public readonly mode = input<'Input' | 'Circle'>('Input');

    public readonly value = signal<string | null>('black');

    public readonly valueParsed = computed(() => {
        return parseColor(this.value());
    });

    public readonly valueForeground = computed(() => {
        if (MathHelper.toLuminance(MathHelper.parseColor(this.valueParsed())!) < 0.5) {
            return 'white';
        }

        return 'black';
    });

    public writeValue(value: any) {
        this.value.set(value);
    }

    public focus() {
        if (this.isDisabled()) {
            return;
        }

        this.inputElement()?.nativeElement?.focus();
    }

    public blur() {
        if (this.isDisabled()) {
            return;
        }

        this.callTouched();
    }

    public updateValue(value: string) {
        if (this.isDisabled()) {
            return;
        }

        if (this.value() !== value) {
            this.callChange(value);
            this.writeValue(value);
        }
    }
}

let canvas: HTMLCanvasElement;
function parseColor(color: string | null) {
    if (!canvas) {
        canvas = document.createElement('canvas');
    }

    const ctx = canvas.getContext('2d')!;
    if (!ctx) {
        return color || '';
    }

    ctx.fillStyle = color || '';
    return ctx.fillStyle;
}