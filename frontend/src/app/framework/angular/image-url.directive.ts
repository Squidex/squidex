/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { DestroyRef, Directive, effect, ElementRef, inject, input, NgZone, Renderer2 } from '@angular/core';

@Directive({
    selector: '[sqxImageUrl]',
    host: {
        '[attr.src]': 'imageUrl()',
    },
})
export class ImageUrlDirective {
    private readonly zone = inject(NgZone);
    private readonly element = inject(ElementRef);
    private readonly renderer = inject(Renderer2);

    /** The URL of the image. The image is hidden until it has been loaded. */
    public readonly imageUrl = input.required<string>({ alias: 'sqxImageUrl' });

    constructor() {
        const unlisteners = this.zone.runOutsideAngular(() => [
            this.renderer.listen(this.element.nativeElement, 'load', () => {
                this.onLoad();
            }),
            this.renderer.listen(this.element.nativeElement, 'error', () => {
                this.onError();
            }),
        ]);

        inject(DestroyRef).onDestroy(() => {
            for (const unlisten of unlisteners) {
                unlisten();
            }
        });

        effect(() => {
            this.imageUrl();
            this.onError();
        });
    }

    private onLoad() {
        this.renderer.setStyle(this.element.nativeElement, 'visibility', 'visible');
    }

    private onError() {
        this.renderer.setStyle(this.element.nativeElement, 'visibility', 'hidden');
    }
}
