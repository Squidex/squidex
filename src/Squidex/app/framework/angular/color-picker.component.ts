/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, ElementRef, EventEmitter, HostListener, Input, OnChanges, Output } from '@angular/core';

import { Color } from './../utils/color';
import { ColorPalette } from './../utils/color-palette';

@Component({
    selector: 'sqx-color-picker',
    styleUrls: ['./color-picker.component.scss'],
    templateUrl: './color-picker.component.html'
})
export class ColorPickerComponent implements OnChanges {
    private selectedColorValue = new Color(0, 0, 0);

    @Output()
    public colorChange = new EventEmitter();

    @Input()
    public color: string | number | Color;

    @Input()
    public palette = ColorPalette.colors();

    @Input()
    public isOpen = false;

    public get selectedColor(): Color {
        return this.selectedColorValue;
    }

    constructor(private readonly element: ElementRef) {
        this.updateColor();
    }

    @HostListener('document:click', ['$event.target'])
    public onClick(targetElement: any) {
        const clickedInside = this.element.nativeElement.contains(targetElement);

        if (!clickedInside) {
            this.close();
        }
    }

    public ngOnChanges() {
        this.updateColor();
    }

    public toggleOpen() {
        this.isOpen = !this.isOpen;
    }

    public close() {
        this.isOpen = false;
    }

    public open() {
        this.isOpen = true;
    }

    public selectColor(color: Color) {
        this.updateParent(color);
        this.updateColor(color);
        this.close();
    }

    private updateParent(color: Color) {
        if (this.selectedColorValue.ne(color)) {
            this.colorChange.emit(color);
        }
    }

    private updateColor(color?: Color) {
        let hasColor = false;
        try {
            this.selectedColorValue = Color.fromValue(color || this.color);

            hasColor = true;
        } catch (e) {
            hasColor = false;
        }

        if (!hasColor || !this.selectedColorValue) {
            if (this.palette) {
                this.selectedColorValue = this.palette.defaultColor;
            } else {
                this.selectedColorValue = Color.BLACK;
            }
        }
    }
}