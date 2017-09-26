/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Directive, ElementRef, HostListener, Input, OnChanges, OnDestroy, OnInit, Renderer } from '@angular/core';

import { MathHelper } from './../utils/math-helper';

@Directive({
    selector: '[sqxImageSource]'
})
export class ImageSourceDirective implements OnChanges, OnDestroy, OnInit, AfterViewInit {
    private parentResizeListener: Function;

    private loadingTimer: any;
    private size: any;
    private loadRetries = 0;
    private loadQuery: string | null = null;

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

    public ngOnDestroy() {
        clearTimeout(this.loadingTimer);

        this.parentResizeListener();
    }

    public ngOnInit() {
        if (this.parent === null) {
            this.parent = this.element.nativeElement.parentElement;
        }

        this.parentResizeListener =
            this.renderer.listen(this.parent, 'resize', () => {
                this.resize(this.parent);
            });
    }

    public ngAfterViewInit() {
        this.resize(this.parent);
    }

    public ngOnChanges() {
        this.loadQuery = null;
        this.loadRetries = 0;

        this.setImageSource();
    }

    @HostListener('load')
    public onLoad() {
        this.renderer.setElementStyle(this.element.nativeElement, 'visibility', 'visible');
    }

    @HostListener('error')
    public onError() {
        this.renderer.setElementStyle(this.element.nativeElement, 'visibility', 'hidden');

        this.retryLoadingImage();
    }

    private resize(parent: any) {
        this.size = this.parent.getBoundingClientRect();

        this.renderer.setElementStyle(this.element.nativeElement, 'display', 'inline-block');
        this.renderer.setElementStyle(this.element.nativeElement, 'width', this.size.width + 'px');
        this.renderer.setElementStyle(this.element.nativeElement, 'height', this.size.height + 'px');

        this.setImageSource();
    }

    private setImageSource() {
        if (!this.size) {
            return;
        }

        const w = Math.round(this.size.width);
        const h = Math.round(this.size.height);

        if (w > 0 && h > 0) {
            let source = `${this.imageSource}&width=${w}&height=${h}&mode=Crop`;

            if (this.loadQuery !== null) {
                source += `&q=${this.loadQuery}`;
            }

            this.renderer.setElementAttribute(this.element.nativeElement, 'src', source);
        }
    }

    private retryLoadingImage() {
        this.loadRetries++;

        if (this.loadRetries <= 10) {
            this.loadingTimer =
                setTimeout(() => {
                    this.loadQuery = MathHelper.guid();

                    this.setImageSource();
                }, this.loadRetries * 1000);
        }
    }
}