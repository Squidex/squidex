/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, inject, input, Renderer2 } from '@angular/core';

@Directive({
    selector: '[sqxSyncScrolling]',
    host: {
        '(scroll)': 'onScroll($event)',
    },
})
export class SyncScollingDirective {
    private readonly renderer = inject(Renderer2);

    /** The element that is moved horizontally with the scroll position of this element. */
    public readonly target = input<HTMLElement | null | undefined>(undefined, { alias: 'sqxSyncScrolling' });

    protected onScroll(event: Event) {
        const target = this.target();
        if (!target) {
            return;
        }

        const scroll = (<HTMLElement>event.target).scrollLeft;

        this.renderer.setStyle(target, 'transform', `translate(-${scroll - target.scrollLeft}px, 0px)`);
    }
}
