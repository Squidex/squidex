/*
 * Athene Requirements Center
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Color } from './../';

import { ColorPickerComponent } from './color-picker.component';

describe('ColorPickerComponent', () => {
    it('should instantiate', () => {
        const colorPicker = new ColorPickerComponent({ nativeElement: {} });

        expect(colorPicker).toBeDefined();
    });

    it('should close color picker when clicking outside of the modal', () => {
        const element = {
            nativeElement: {
                contains: () => {
                    return false;
                }
            }
        };

        const colorPicker = new ColorPickerComponent(element);
        colorPicker.open();
        colorPicker.onClick({});

        expect(colorPicker.isOpen).toBeFalsy();
    });

    it('should not close color picker when clicking inside the modal', () => {
        const element = {
            nativeElement: {
                contains: () => {
                    return true;
                }
            }
        };

        const colorPicker = new ColorPickerComponent(element);
        colorPicker.open();
        colorPicker.onClick({});

        expect(colorPicker.isOpen).toBeTruthy();
    });

    it('should close modal and emit event when setting color', () => {
        const colorPicker = new ColorPickerComponent({ nativeElement: {} });
        const selectedColor = Color.RED;

        let lastColor: Color | null = null;

        colorPicker.colorChange.subscribe((c: Color) => {
            lastColor = c;
        });

        colorPicker.open();
        colorPicker.selectColor(selectedColor);

        expect(lastColor).toBe(selectedColor);
        expect(colorPicker.isOpen).toBeFalsy();
    });

    it('should not emit event when selecting same color', () => {
        const colorPicker = new ColorPickerComponent({ nativeElement: {} });
        const selectedColor = Color.RED;

        colorPicker.selectColor(selectedColor);

        let lastColor: Color | null = null;

        colorPicker.colorChange.subscribe((c: Color) => {
            lastColor = c;
        });

        colorPicker.selectColor(selectedColor);

        expect(lastColor).toBeNull();
    });

    it('should update selected color when component changes', () => {
        const colorPicker = new ColorPickerComponent({ nativeElement: {} });
        const selectedColor = Color.RED;

        colorPicker.color = selectedColor;

        colorPicker.ngOnChanges({});

        expect(colorPicker.selectedColor).toBe(selectedColor);
    });

    it('should update selected color with palette default if setting invalid color', () => {
        const colorPicker = new ColorPickerComponent({ nativeElement: {} });

        colorPicker.color = 'invalid';

        colorPicker.ngOnChanges({});

        expect(colorPicker.selectedColor).toBe(colorPicker.palette.defaultColor);
    });

    it('should update selected color with black if setting invalid color and palette is null', () => {
        const colorPicker = new ColorPickerComponent({ nativeElement: {} });

        colorPicker.palette = undefined!;
        colorPicker.color = 'invalid';

        colorPicker.ngOnChanges({});

        expect(colorPicker.selectedColor).toBe(Color.BLACK);
    });

    it('should update isOpen prperty when toggleOpen is invoked', () => {
        const colorPicker = new ColorPickerComponent({ nativeElement: {} });

        colorPicker.toggleOpen();

        expect(colorPicker.isOpen).toBeTruthy();
    });
});