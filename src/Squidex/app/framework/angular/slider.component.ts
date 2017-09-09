/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, ElementRef, forwardRef, Input, Renderer, ViewChild } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

import { Types } from './../utils/types';

export const SQX_SLIDER_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => SliderComponent), multi: true
};

@Component({
    selector: 'sqx-slider',
    styleUrls: ['./slider.component.scss'],
    templateUrl: './slider.component.html',
    providers: [SQX_SLIDER_CONTROL_VALUE_ACCESSOR]
})
export class SliderComponent implements ControlValueAccessor {
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };
    private mouseMoveSubscription: Function | null = null;
    private mouseUpSubscription: Function | null = null;
    private centerStartOffset = 0;
    private startValue: number;
    private lastValue: number;
    private value: number;
    private isDragging = false;

    public isDisabled = false;

    @ViewChild('bar')
    public bar: ElementRef;

    @ViewChild('thumb')
    public thumb: ElementRef;

    @Input()
    public min = 0;

    @Input()
    public max = 100;

    @Input()
    public step = 1;

    constructor(private readonly renderer: Renderer) { }

    public writeValue(value: number) {
        this.lastValue = this.value = Types.isNumber(value) ? value : 0;

        this.updateThumbPosition();
    }

    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;
    }

    public registerOnChange(fn: any) {
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
    }

    public onBarMouseClick(event: MouseEvent): boolean {
        if (this.mouseMoveSubscription) {
            return true;
        }

        const relativeValue = this.getRelativeX(event);

        this.value = Math.round((relativeValue * (this.max - this.min) + this.min) / this.step) * this.step;

        this.updateThumbPosition();
        this.updateTouched();
        this.updateValue();

        return false;
    }

    public onThumbMouseDown(event: MouseEvent): boolean {
        this.centerStartOffset = event.offsetX - this.thumb.nativeElement.clientWidth * 0.5;

        this.startValue = this.value;

        this.mouseMoveSubscription =
            this.renderer.listenGlobal('window', 'mousemove', (e: MouseEvent) => {
                this.onMouseMove(e);
            });

        this.mouseUpSubscription =
            this.renderer.listenGlobal('window', 'mouseup', () => {
                this.onMouseUp();
            });

        this.renderer.setElementClass(this.thumb.nativeElement, 'focused', true);

        this.isDragging = true;

        return false;
    }

    private onMouseMove(event: MouseEvent): boolean {
        if (!this.isDragging) {
            return true;
        }

        const relativeValue = this.getRelativeX(event);

        this.value = Math.round((relativeValue * (this.max - this.min) + this.min) / this.step) * this.step;

        this.updateThumbPosition();
        this.updateTouched();

        return false;
    }

    private onMouseUp(): boolean {
        this.updateValue();

        setTimeout(() => {
            this.releaseMouseHandlers();
            this.renderer.setElementClass(this.thumb.nativeElement, 'focused', false);
        }, 10);

        this.centerStartOffset = 0;

        this.isDragging = false;

        return false;
    }

    private getRelativeX(event: MouseEvent): number {
        const parentOffsetX = this.getParentX(event, this.bar.nativeElement) - this.centerStartOffset;
        const parentWidth = this.bar.nativeElement.clientWidth;

        const relativeValue = Math.min(1, Math.max(0, (parentOffsetX - this.centerStartOffset) / parentWidth));

        return relativeValue;
    }

    private getParentX(e: any, container: any): number {
        const rect = container.getBoundingClientRect();

        const x =
            !!e.touches ?
                e.touches[0].pageX :
                e.pageX;

        return x - rect.left;
    }

    private updateTouched() {
        this.callTouched();
    }

    private updateValue() {
        if (this.lastValue !== this.value) {
            this.lastValue = this.value;

            this.callChange(this.value);
        }
    }

    private updateThumbPosition() {
        const relativeValue = Math.min(1, Math.max(0, (this.value - this.min) / (this.max - this.min)));

        this.renderer.setElementStyle(this.thumb.nativeElement, 'left', relativeValue * 100 + '%');
    }

    private releaseMouseHandlers() {
        if (this.mouseMoveSubscription) {
            this.mouseMoveSubscription();
            this.mouseMoveSubscription = null;
        }

        if (this.mouseUpSubscription) {
            this.mouseUpSubscription();
            this.mouseUpSubscription = null;
        }

        this.isDragging = false;
    }
}