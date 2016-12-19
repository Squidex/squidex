/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, ElementRef, forwardRef, HostListener, Input } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

import { Color } from './../utils/color';
import { ColorPalette } from './../utils/color-palette';

/* tslint:disable:no-empty */

const NOOP = () => { };

export const SQX_COLOR_PICKER_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => ColorPickerComponent),
    multi: true
};

@Component({
    selector: 'sqx-color-picker',
    styleUrls: ['./color-picker.component.scss'],
    templateUrl: './color-picker.component.html',
    providers: [SQX_COLOR_PICKER_CONTROL_VALUE_ACCESSOR]
})
export class ColorPickerComponent implements ControlValueAccessor {
    private changeCallback: (value: any) => void = NOOP;
    private touchedCallback: () => void = NOOP;

    public selectedColor: Color = Color.BLACK;

    @Input()
    public palette = ColorPalette.colors();

    @Input()
    public dropdownSide: 'left';

    @Input()
    public isOpen = false;

    constructor(private readonly element: ElementRef) {
        this.updateColor();
    }

    public writeValue(value: any) {
        this.updateColor(value);
    }

    public setDisabledState(isDisabled: boolean): void {
    }

    public registerOnChange(fn: any) {
        this.changeCallback = fn;
    }

    public registerOnTouched(fn: any) {
        this.touchedCallback = fn;
    }

    @HostListener('document:click', ['$event.target'])
    public onClick(targetElement: any) {
        const clickedInside = this.element.nativeElement.contains(targetElement);

        if (!clickedInside) {
            this.close();
        }
    }

    public toggleOpen() {
        if (this.isOpen) {
            this.close();
        } else {
            this.open();
        }
    }

    public open() {
        this.isOpen = true;
    }

    public close() {
        this.isOpen = false;

        this.touchedCallback();
    }

    public selectColor(color: Color) {
        this.updateParent(color);
        this.updateColor(color);
        this.close();
    }

    private updateParent(color: Color) {
        if (this.selectedColor.ne(color)) {
            this.changeCallback(color);
        }
    }

    private updateColor(color?: Color) {
        let hasColor = false;
        try {
            this.selectedColor = Color.fromValue(color);

            hasColor = true;
        } catch (e) {
            hasColor = false;
        }

        if (!hasColor || !this.selectedColor) {
            if (this.palette) {
                this.selectedColor = this.palette.defaultColor;
            } else {
                this.selectedColor = Color.BLACK;
            }
        }
    }
}