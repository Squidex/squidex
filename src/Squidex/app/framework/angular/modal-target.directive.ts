/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Directive, ElementRef, Input, OnDestroy, OnInit, Renderer } from '@angular/core';
import { Observable, Subscription } from 'rxjs';

@Directive({
    selector: '[sqxModalTarget]'
})
export class ModalTargetDirective implements AfterViewInit, OnDestroy, OnInit {
    private elementResizeListener: Function;
    private targetResizeListener: Function;
    private timer: Subscription;
    private targetElement: any;

    @Input('sqxModalTarget')
    public target: any;

    @Input()
    public offset = 2;

    @Input()
    public position = 'topLeft';

    @Input()
    public auto = true;

    constructor(
        private readonly renderer: Renderer,
        private readonly element: ElementRef
    ) {
    }

    public ngOnInit() {
        if (this.target) {
            this.targetElement = this.target;

            this.targetResizeListener =
                this.renderer.listen(this.targetElement, 'resize', () => {
                    this.updatePosition();
                });

            this.elementResizeListener =
                this.renderer.listen(this.element.nativeElement, 'resize', () => {
                    this.updatePosition();
                });

            this.timer =
                Observable.timer(100, 100).subscribe(() => {
                    this.updatePosition();
                });
        }
    }

    public ngOnDestroy() {
        if (this.targetResizeListener) {
            this.targetResizeListener();
        }

        if (this.elementResizeListener) {
            this.elementResizeListener();
        }

        if (this.timer) {
            this.timer.unsubscribe();
        }
    }

    public ngAfterViewInit() {
        const modalRef  = this.element.nativeElement;

        this.renderer.setElementStyle(modalRef, 'position', 'fixed');
        this.renderer.setElementStyle(modalRef, 'z-index', '1000000');

        this.updatePosition();
    }

    private updatePosition() {
        const viewportHeight = document.documentElement.clientHeight;
        const viewportWidth = document.documentElement.clientWidth;

        const modalRef  = this.element.nativeElement;
        const modalRect = this.element.nativeElement.getBoundingClientRect();

        const targetRect: ClientRect = this.targetElement.getBoundingClientRect();

        let top = 0, left = 0;

        if (this.position === 'topLeft' || this.position === 'topRight') {
            top = targetRect.bottom + this.offset;

            if (this.auto && top + modalRect.height > viewportHeight) {
                const t = targetRect.top - modalRect.height - this.offset;

                if (t > 0) {
                    top = t;
                }
            }
        } else {
            top = targetRect.top - modalRect.height - this.offset;

            if (this.auto && top < 0) {
                const t = targetRect.bottom + this.offset;

                if (t + modalRect.height > viewportHeight) {
                    top = t;
                }
            }
        }

        if (this.position === 'topLeft' || this.position === 'bottomLeft') {
            left = targetRect.left + this.offset;

            if (this.auto && left + modalRect.width > viewportWidth) {
                const l = targetRect.right - modalRect.width - this.offset;

                if (l > 0) {
                    left = l;
                }
            }
        } else {
            left = targetRect.right - modalRect.width - this.offset;

            if (this.auto && left < 0) {
                const l = targetRect.left + this.offset;

                if (l + modalRect.width > viewportWidth) {
                    left = l;
                }
            }
        }

        this.renderer.setElementStyle(modalRef, 'top', top + 'px');
        this.renderer.setElementStyle(modalRef, 'left', left + 'px');
        this.renderer.setElementStyle(modalRef, 'right', 'auto');
        this.renderer.setElementStyle(modalRef, 'bottom', 'auto');
        this.renderer.setElementStyle(modalRef, 'margin', '0');
    }
}