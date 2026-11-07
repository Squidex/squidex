/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, ElementRef, input, model, viewChild } from '@angular/core';
import { MathHelper } from '@app/framework/internal';
import { FormValueControlBase } from '../form-value-control';
import { FocusComponent } from '../forms-helper';

let canvas: HTMLCanvasElement;

@Component({
    selector: 'sqx-color-picker',
    styleUrls: ['./color-picker.component.scss'],
    templateUrl: './color-picker.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ColorPickerComponent extends FormValueControlBase<string | null> implements FocusComponent {
    /** The color value. */
    public readonly value = model<string | null>(null);

    /** The placeholder of the text input. */
    public readonly placeholder = input('');

    /** Renders a text input or only a colored circle. */
    public readonly mode = input<'Input' | 'Circle'>('Input');

    protected readonly inputElement = viewChild<ElementRef<HTMLElement>>('input');

    protected readonly valueParsed = computed(() => {
        const color = this.value();
        if (!canvas) {
            canvas = document.createElement('canvas');
        }

        const ctx = canvas.getContext('2d')!;
        if (!ctx) {
            return color || '';
        }

        ctx.fillStyle = color || '';
        return ctx.fillStyle;
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

        this.markTouched();
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
