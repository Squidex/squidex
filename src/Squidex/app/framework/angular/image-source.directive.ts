/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, ElementRef, HostListener, Input, OnChanges, OnInit, Renderer } from '@angular/core';

import { MathHelper } from './../utils/math-helper';

@Directive({
    selector: '[sqxImageSource]'
})
export class ImageSourceComponent implements OnChanges, OnInit {
    private retries = 0;
    private query = MathHelper.guid();

    @Input('sqxImageSource')
    public imageSource: string;

    @Input()
    public retryCount = 10;

    @Input()
    public parent: any = null;

    constructor(
        private readonly elementRef: ElementRef,
        private readonly renderer: Renderer
    ) {
    }

    public ngOnChanges() {
        this.retries = 0;

        this.setImageSource();
    }

    public ngOnInit() {
        if (this.parent === null) {
            this.parent = this.elementRef.nativeElement.parentElement;
        }

        this.resize(this.parent);

        this.renderer.listen(this.parent, 'resize', () => {
            this.resize(this.parent);
        });
    }

    @HostListener('error')
    public onError() {
        this.renderer.setElementStyle(this.elementRef.nativeElement, 'visibility', 'hidden');

        this.retryLoadingImage();
    }

    @HostListener('resize')
    public onResize() {
        this.setImageSource();
    }

    @HostListener('load')
    public onLoad() {
        this.renderer.setElementStyle(this.elementRef.nativeElement, 'visibility', 'visible');
    }

    private resize(parent: any) {
        const size = parent.getBoundingClientRect();

        this.renderer.setElementStyle(this.elementRef.nativeElement, 'width', size.width + 'px');
        this.renderer.setElementStyle(this.elementRef.nativeElement, 'height', size.height + 'px');
    }

    private setImageSource() {
        const size = this.elementRef.nativeElement.getBoundingClientRect();

        const w = Math.round(size.width);
        const h = Math.round(size.height);

        const source = `${this.imageSource}&width=${w}&height=${h}&mode=Crop&q=${this.query}`;

        this.renderer.setElementAttribute(this.elementRef.nativeElement, 'src', source);
    }

    private retryLoadingImage() {
        this.retries++;

        if (this.retries <= 10) {
            setTimeout(() => {
                this.query = MathHelper.guid();

                this.setImageSource();
            }, this.retries * 1000);
        }
    }
}