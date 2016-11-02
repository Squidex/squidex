/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import {
     Color }  
from './../utils/color';

import { 
    ColorPalette 
} from './../utils/color-palette';

@Ng2.Component({
    selector: 'sqx-color-picker',
    styles,
    template
})
export class ColorPickerComponent implements Ng2.OnChanges {
    private selectedColorValue = new Color(0, 0, 0);

    @Ng2.Output()
    public colorChange = new Ng2.EventEmitter();

    @Ng2.Input()
    public color: string | number | Color;

    @Ng2.Input()
    public palette = ColorPalette.colors();

    @Ng2.Input()
    public isOpen = false;

    public get selectedColor(): Color {
        return this.selectedColorValue;
    }

    constructor(private readonly element: Ng2.ElementRef) {
        this.updateColor();
    }

    @Ng2.HostListener('document:click', ['$event.target'])
    public onClick(targetElement: any) {
        const clickedInside = this.element.nativeElement.contains(targetElement);

        if (!clickedInside) {
            this.close();
        }
    }

    public ngOnChanges(changes: Ng2.SimpleChanges) {
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