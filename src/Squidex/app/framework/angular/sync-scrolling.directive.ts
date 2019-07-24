/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, HostListener, Input, OnInit, Renderer2 } from '@angular/core';

@Directive({
    selector: '[sqxSyncScrolling]'
})
export class SyncScollingDirective implements OnInit {
    @Input('sqxSyncScrolling')
    public target: HTMLElement;

    constructor(
        private readonly renderer: Renderer2
    ) {
    }

    public ngOnInit() {
        if (this.target) {
            this.renderer.setStyle(this.target, 'overflow-x', 'hidden');
        }
    }

    @HostListener('scroll', ['$event'])
    public onScroll(event: Event) {
        if (this.target) {
            const scroll = (<HTMLElement>event.target).scrollLeft;

            this.target.scrollLeft = scroll;
        }
    }
}