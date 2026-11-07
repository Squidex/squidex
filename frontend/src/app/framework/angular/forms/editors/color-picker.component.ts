/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, ElementRef, input, model, viewChild } from '@angular/core';
import { FormValueControl } from '@angular/forms/signals';
import { MathHelper } from '@app/framework/internal';
import { FocusComponent } from '../forms-helper';

@Component({
    selector: 'sqx-color-picker',
    styleUrls: ['./color-picker.component.scss'],
    templateUrl: './color-picker.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ColorPickerComponent implements FormValueControl<string | null>, FocusComponent {
    private readonly inputElement = viewChild<ElementRef<HTMLElement>>('input');

    /** The color value. */
    public readonly value = model<string | null>(null);

    /** Disables the control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Indicates whether the control has been touched. */
    public readonly touched = model(false);

    /** The placeholder of the text input. */
    public readonly placeholder = input('');

    /** Renders a text input or only a colored circle. */
    public readonly mode = input<'Input' | 'Circle'>('Input');

    protected readonly valueParsed = computed(() => {
        return parseColor(this.value());
    });

    protected readonly valueForeground = computed(() => {
        if (MathHelper.toLuminance(MathHelper.parseColor(this.valueParsed())!) < 0.5) {
            return 'white';
        }

        return 'black';
    });

    public focus() {
        if (this.disabled()) {
            return;
        }

        this.inputElement()?.nativeElement?.focus();
    }

    protected blur() {
        if (this.disabled()) {
            return;
        }

        this.touched.set(true);
    }

    protected updateValue(value: string) {
        if (this.disabled()) {
            return;
        }

        if (this.value() !== value) {
            this.value.set(value);
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
