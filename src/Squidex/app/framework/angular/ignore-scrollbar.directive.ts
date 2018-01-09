/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, OnDestroy, OnInit, Renderer } from '@angular/core';

@Directive({
    selector: '[sqxIgnoreScrollbar]'
})
export class IgnoreScrollbarDirective implements OnDestroy, OnInit, AfterViewInit {
    private resizeListener: Function;
    private parent: any;
    private checkTimer: any;
    private scollbarWidth = 0;

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer
    ) {
    }

    public ngOnDestroy() {
        clearTimeout(this.checkTimer);

        this.resizeListener();
    }

    public ngOnInit() {
        if (!this.parent) {
            this.parent = this.element.nativeElement.parentElement;
        }

        this.resizeListener =
            this.renderer.listen(this.element.nativeElement, 'resize', () => {
                this.reposition();
            });

        this.checkTimer =
            setTimeout(() => {
                this.reposition();
            }, 100);
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

            this.renderer.setElementStyle(this.element.nativeElement, 'marginRight', `-${scrollbarWidth}px`);
        }
    }
}