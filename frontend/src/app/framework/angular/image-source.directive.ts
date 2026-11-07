/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterNextRender, computed, DestroyRef, Directive, effect, ElementRef, inject, input, NgZone, numberAttribute, Renderer2 } from '@angular/core';
import { MathHelper, StringHelper } from '@app/framework/internal';

const LAYOUT_CACHE: { [key: string]: { width: number; height: number } } = {};

@Directive({
    selector: '[sqxImageSource]',
})
export class ImageSourceDirective {
    private readonly destroyRef = inject(DestroyRef);
    private readonly element = inject(ElementRef);
    private readonly renderer = inject(Renderer2);
    private readonly zone = inject(NgZone);
    private loadQuery: string | null = null;
    private loadRetries = 0;
    private loadTimer: any;
    private size: any;

    /** The URL of the image, without the size query. */
    public readonly imageSource = input.required<string>({ alias: 'sqxImageSource' });

    /** The number of retries when the image cannot be loaded. */
    public readonly retryCount = input(0, { transform: numberAttribute });

    /** The key to cache and share the size between images with the same layout. */
    public readonly layoutKey = input('');

    /** The element to calculate the size from. Defaults to the parent element. */
    public readonly parent = input<any>(null);

    protected readonly parentElement = computed(() => {
        return this.parent() || this.renderer.parentNode(this.element.nativeElement);
    });

    constructor() {
        effect(onCleanup => {
            const parent = this.parentElement();

            const unlisteners = this.zone.runOutsideAngular(() => [
                this.renderer.listen(parent, 'resize', () => {
                    this.resize();
                }),
                this.renderer.listen(this.element.nativeElement, 'load', () => {
                    this.onLoad();
                }),
                this.renderer.listen(this.element.nativeElement, 'error', () => {
                    this.onError();
                }),
            ]);

            onCleanup(() => {
                for (const unlisten of unlisteners) {
                    unlisten();
                }
            });
        });

        effect(() => {
            this.loadQuery = null;
            this.loadRetries = 0;

            this.setImageSource();
        });

        afterNextRender(() => {
            this.resize();
        });

        this.destroyRef.onDestroy(() => {
            clearTimeout(this.loadTimer);
        });
    }

    private onLoad() {
        this.renderer.setStyle(this.element.nativeElement, 'visibility', 'visible');
    }

    private onError() {
        this.renderer.setStyle(this.element.nativeElement, 'visibility', 'hidden');

        this.retryLoadingImage();
    }

    private resize() {
        let size: { width: number; height: number } = null!;

        const layoutKey = this.layoutKey();
        if (layoutKey) {
            size = LAYOUT_CACHE[layoutKey];
        }

        if (!size) {
            const parent = this.parentElement();

            size = { width: parent.offsetWidth, height: parent.offsetHeight };
        }

        this.size = size;

        if (layoutKey) {
            LAYOUT_CACHE[layoutKey] = size;
        }

        this.renderer.setStyle(this.element.nativeElement, 'display', 'inline-block');
        this.renderer.setStyle(this.element.nativeElement, 'width', `${this.size.width}px`);
        this.renderer.setStyle(this.element.nativeElement, 'height', `${this.size.height}px`);

        this.setImageSource();
    }

    private setImageSource() {
        const imageSource = this.imageSource();
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

            const url = imageSource + query;

            this.renderer.setProperty(this.element.nativeElement, 'src', url);
        }
    }

    private retryLoadingImage() {
        this.loadRetries++;

        if (this.loadRetries <= this.retryCount()) {
            this.loadTimer =
                setTimeout(() => {
                    this.loadQuery = MathHelper.guid();

                    this.setImageSource();
                }, this.loadRetries * 1000);
        }
    }
}
