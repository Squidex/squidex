/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, ElementRef, EventEmitter, Input, OnChanges, Output, Renderer, ViewChild } from '@angular/core';

@Component({
    selector: 'sqx-slider',
    styleUrls: ['./slider.component.scss'],
    templateUrl: './slider.component.html'
})
export class SliderComponent implements OnChanges {
    private mouseMoveSubscription: Function | null;
    private mouseUpSubscription: Function | null;
    private centerStartOffset = 0;
    private startValue: number;

    @ViewChild('bar')
    public bar: ElementRef;

    @ViewChild('thumb')
    public thumb: ElementRef;

    @Input()
    public min = 0;

    @Input()
    public max = 100;

    @Input()
    public value: number;

    @Output()
    public valueChange = new EventEmitter();

    constructor(private readonly renderer: Renderer) { }

    public ngOnChanges() {
        const relativeValue = (this.value - this.min) / (this.max - this.min);

        this.setThumbPosition(relativeValue);
    }

    public onBarMouseClick(event: MouseEvent) {
        const relativeValue = this.getRelativeX(event);

        this.setThumbPosition(relativeValue);

        const newValue = Math.round(relativeValue * (this.max - this.min) + this.min);

        if (newValue !== this.value) {
            this.valueChange.emit(newValue);
        }

        this.stopEvent(event);
    }

    public onThumbMouseDown(event: MouseEvent) {
        this.centerStartOffset = event.offsetX - this.thumb.nativeElement.clientWidth * 0.5;

        this.startValue = this.value;

        this.mouseMoveSubscription =
            this.renderer.listenGlobal('window', 'mousemove', (e: MouseEvent) => {
                this.onMouseMove(e);
            });

        this.mouseUpSubscription =
            this.renderer.listenGlobal('window', 'mouseup', (e: MouseEvent) => {
                this.onMouseUp(e);
            });

        this.renderer.setElementClass(this.thumb.nativeElement, 'focused', true);

        this.stopEvent(event);
    }

    private onMouseMove(event: MouseEvent) {
        const relativeValue = this.getRelativeX(event);

        this.setThumbPosition(relativeValue);

        this.stopEvent(event);
    }

    private onMouseUp(event: MouseEvent) {
        const relativeValue = this.getRelativeX(event);

        const newValue = Math.round(relativeValue * (this.max - this.min) + this.min);

        if (newValue !== this.startValue) {
            this.valueChange.emit(newValue);
        }

        this.releaseMouseHandlers();
        this.renderer.setElementClass(this.thumb.nativeElement, 'focused', false);

        this.centerStartOffset = 0;

        this.stopEvent(event);
    }

    private getRelativeX(event: MouseEvent): number {
        const parentOffsetX = this.getParentX(event, this.bar.nativeElement) - this.centerStartOffset;
        const parentWidth = this.bar.nativeElement.clientWidth;

        const relativeValue = Math.min(1, Math.max(0, (parentOffsetX - this.centerStartOffset) / parentWidth));

        return relativeValue;
    }

    private getParentX(e: any, container: any): number {
        const rect = container.getBoundingClientRect();

        const x = !!e.touches ? e.touches[0].pageX : e.pageX;

        return x - rect.left;
    }

    private setThumbPosition(relativeValue: number) {
        relativeValue = Math.min(1, Math.max(0, relativeValue));

        this.renderer.setElementStyle(this.thumb.nativeElement, 'left', relativeValue * 100 + '%');
    }

    private stopEvent(event: Event) {
        event.preventDefault();
        event.stopPropagation();
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
    }
}