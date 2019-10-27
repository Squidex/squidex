/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, HostListener, OnInit, Renderer2 } from '@angular/core';
import { timer } from 'rxjs';

import { ResourceOwner } from '@app/framework/internal';

@Directive({
    selector: '[sqxIgnoreScrollbar]'
})
export class IgnoreScrollbarDirective extends ResourceOwner implements OnInit, AfterViewInit {
    private parent: any;
    private scollbarWidth = 0;

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer2
    ) {
        super();
    }

    @HostListener('resize)')
    public onResize() {
        this.reposition();
    }

    public ngOnInit() {
        this.parent = this.renderer.parentNode(this.element.nativeElement);

        this.own(timer(100, 100).subscribe(() => this.reposition));
    }

    public ngAfterViewInit() {
        this.reposition();
    }

    private reposition() {
        if (!this.parent) {
            return;
        }

        const parentOuter = this.parent.offsetWidth;
        const parentInner = this.parent.clientWidth;

        const scrollbarWidth = parentOuter - parentInner;

        if (scrollbarWidth !== this.scollbarWidth) {
            this.scollbarWidth = scrollbarWidth;

            this.renderer.setStyle(this.element.nativeElement, 'marginRight', `-${scrollbarWidth}px`);
        }
    }
}