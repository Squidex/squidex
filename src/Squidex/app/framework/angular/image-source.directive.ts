/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, HostListener, Input, OnChanges, OnDestroy, OnInit, Renderer2 } from '@angular/core';

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
        private readonly renderer: Renderer2
    ) {
    }

    public ngOnDestroy() {
        clearTimeout(this.loadingTimer);

        this.parentResizeListener();
    }

    public ngOnInit() {
        if (!this.parent) {
            this.parent = this.renderer.parentNode(this.element.nativeElement);
        }

        this.parentResizeListener =
            this.renderer.listen(this.parent, 'resize', () => {
                this.resize();
            });
    }

    public ngAfterViewInit() {
        this.resize();
    }

    public ngOnChanges() {
        this.loadQuery = null;
        this.loadRetries = 0;

        this.setImageSource();
    }

    @HostListener('load')
    public onLoad() {
        this.renderer.setStyle(this.element.nativeElement, 'visibility', 'visible');
    }

    @HostListener('error')
    public onError() {
        this.renderer.setStyle(this.element.nativeElement, 'visibility', 'hidden');

        this.retryLoadingImage();
    }

    private resize() {
        this.size = this.parent.getBoundingClientRect();

        this.renderer.setStyle(this.element.nativeElement, 'display', 'inline-block');
        this.renderer.setStyle(this.element.nativeElement, 'width', this.size.width + 'px');
        this.renderer.setStyle(this.element.nativeElement, 'height', this.size.height + 'px');

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

            this.renderer.setProperty(this.element.nativeElement, 'src', source);
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