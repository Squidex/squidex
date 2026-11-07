/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, HostListener, Input, Renderer2 } from '@angular/core';

@Directive({
    selector: '[sqxSyncScrolling]',
})
export class SyncScollingDirective {
    @Input('sqxSyncScrolling')
    public target!: HTMLElement;

    constructor(
        private readonly renderer: Renderer2,
    ) {
    }

    @HostListener('scroll', ['$event'])
    protected onScroll(event: Event) {
        if (this.target) {
            const scroll = (<HTMLElement>event.target).scrollLeft;

            this.renderer.setStyle(this.target, 'transform', `translate(-${scroll - this.target.scrollLeft}px, 0px)`);
        }
    }
}
