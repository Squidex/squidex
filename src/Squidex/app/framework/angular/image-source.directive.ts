/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Directive, ElementRef, HostListener, Input, OnChanges, OnInit, Renderer } from '@angular/core';

import { MathHelper } from './../utils/math-helper';

@Directive({
    selector: '[sqxImageSource]'
})
export class ImageSourceDirective implements OnChanges, OnInit, AfterViewInit {
    private retries = 0;
    private query: string | null = null;

    @Input('sqxImageSource')
    public imageSource: string;

    @Input()
    public retryCount = 10;

    @Input()
    public parent: any = null;

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer
    ) {
    }

    public ngOnChanges() {
        this.query = null;
        this.retries = 0;

        this.setImageSource();
    }

    public ngAfterViewInit() {
        this.resize(this.parent);
    }

    public ngOnInit() {
        this.renderer.setElementStyle(this.element.nativeElement, 'display', 'inline-block');

        if (this.parent === null) {
            this.parent = this.element.nativeElement.parentElement;
        }

        this.resize(this.parent);

        this.renderer.listen(this.parent, 'resize', () => {
            this.resize(this.parent);
        });
    }

    @HostListener('error')
    public onError() {
        this.renderer.setElementStyle(this.element.nativeElement, 'visibility', 'hidden');

        this.retryLoadingImage();
    }

    @HostListener('resize')
    public onResize() {
        this.setImageSource();
    }

    @HostListener('load')
    public onLoad() {
        this.renderer.setElementStyle(this.element.nativeElement, 'visibility', 'visible');
    }

    private resize(parent: any) {
        const size = parent.getBoundingClientRect();

        this.renderer.setElementStyle(this.element.nativeElement, 'width', size.width + 'px');
        this.renderer.setElementStyle(this.element.nativeElement, 'height', size.height + 'px');

        this.setImageSource();
    }

    private setImageSource() {
        const size = this.element.nativeElement.getBoundingClientRect();

        const w = Math.round(size.width);
        const h = Math.round(size.height);

        if (w > 0 && h > 0) {
            let source = `${this.imageSource}&width=${w}&height=${h}&mode=Crop`;

            if (this.query !== null) {
                source += `&q=${this.query}`;
            }

            this.renderer.setElementAttribute(this.element.nativeElement, 'src', source);
        }
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