/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterNextRender, Directive, DestroyRef, ElementRef, inject, input, Renderer2 } from '@angular/core';
import { ResizeListener, ResizeService } from '@app/framework/internal';

@Directive({
    selector: '[sqxSyncWidth]',
})
export class SyncWidthDirective implements ResizeListener {
    private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);
    private readonly renderer = inject(Renderer2);
    private readonly resizeService = inject(ResizeService);

    /** The element that gets the same width as this element. */
    public readonly target = input<HTMLElement | null | undefined>(undefined, { alias: 'sqxSyncWidth' });

    constructor() {
        const unlisten = this.resizeService.listen(this.element.nativeElement, this);

        inject(DestroyRef).onDestroy(unlisten);

        afterNextRender(() => {
            this.resize(this.element.nativeElement.clientWidth);
        });
    }

    public onResize(size: DOMRect) {
        this.resize(size.width);
    }

    private resize(width: number) {
        const target = this.target();
        if (!target) {
            return;
        }

        this.renderer.setStyle(target, 'width', `${width}px`);
    }
}
