/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, Input, NgZone, numberAttribute, OnDestroy, OnInit, Renderer2 } from '@angular/core';
import { MathHelper, StringHelper, Subscriptions } from '@app/framework/internal';

const LAYOUT_CACHE: { [key: string]: { width: number; height: number } } = {};

@Directive({
    selector: '[sqxImageSource]',
})

export class ImageSourceDirective implements OnDestroy, OnInit, AfterViewInit {
    private readonly subscriptions = new Subscriptions();
    private size: any;
    private loadTimer: any;
    private loadRetries = 0;
    private loadQuery: string | null = null;

    @Input('sqxImageSource')
    public imageSource!: string;

    @Input({ transform: numberAttribute })
    public retryCount = 0;

    @Input()
    public layoutKey = '';

    @Input()
    public parent: any = null;

    constructor(
        private readonly zone: NgZone,
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
    }

    public ngOnDestroy() {
        clearTimeout(this.loadTimer);
    }

    public ngOnInit() {
        if (!this.parent) {
            this.parent = this.renderer.parentNode(this.element.nativeElement);
        }

        this.zone.runOutsideAngular(() => {
            this.subscriptions.add(
                this.renderer.listen(this.parent, 'resize', () => {
                    this.resize();
                }));

            this.subscriptions.add(
                this.renderer.listen(this.element.nativeElement, 'load', () => {
                    this.onLoad();
                }));

            this.subscriptions.add(
                this.renderer.listen(this.element.nativeElement, 'error', () => {
                    this.onError();
                }));
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

    public onLoad() {
        this.renderer.setStyle(this.element.nativeElement, 'visibility', 'visible');
    }

    public onError() {
        this.renderer.setStyle(this.element.nativeElement, 'visibility', 'hidden');

        this.retryLoadingImage();
    }

    private resize() {
        let size: { width: number; height: number } = null!;

        if (this.layoutKey) {
            size = LAYOUT_CACHE[this.layoutKey];
        }

        if (!size) {
            size = { width: this.parent.offsetWidth, height: this.parent.offsetHeight };
        }

        this.size = size;

        if (this.layoutKey) {
            LAYOUT_CACHE[this.layoutKey] = size;
        }

        this.renderer.setStyle(this.element.nativeElement, 'display', 'inline-block');
        this.renderer.setStyle(this.element.nativeElement, 'width', `${this.size.width}px`);
        this.renderer.setStyle(this.element.nativeElement, 'height', `${this.size.height}px`);

        this.setImageSource();
    }

    private setImageSource() {
        if (!this.size) {
            return;
        }

        const w = Math.round(this.size.width);
        const h = Math.round(this.size.height);

        if (w > 0 && h > 0) {
            const query = StringHelper.buildQuery({
                width: w,
                height: h,
                mode: 'Pad',
                nofocus: 'nofocus',
                q: this.loadQuery,
            });

            const url = this.imageSource + query;

            this.renderer.setProperty(this.element.nativeElement, 'src', url);
        }
    }

    private retryLoadingImage() {
        this.loadRetries++;

        if (this.loadRetries <= this.retryCount) {
            this.loadTimer =
                setTimeout(() => {
                    this.loadQuery = MathHelper.guid();

                    this.setImageSource();
                }, this.loadRetries * 1000);
        }
    }
}
