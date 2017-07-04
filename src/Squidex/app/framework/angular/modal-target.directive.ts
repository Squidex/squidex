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
    public position = 'left';

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

        const modalRef  = this.element.nativeElement;
        const modalRect = this.element.nativeElement.getBoundingClientRect();

        const targetRect: ClientRect = this.targetElement.getBoundingClientRect();

        const left = this.position === 'left' ?
            targetRect.left :
            targetRect.right - modalRect.width;

        let top = targetRect.bottom + this.offset;

        if (top + modalRect.height > viewportHeight) {
            const potentialTop = targetRect.top - modalRect.height - this.offset;

            if (potentialTop > 0) {
                top = potentialTop;
            }
        }

        this.renderer.setElementStyle(modalRef, 'top', top + 'px');
        this.renderer.setElementStyle(modalRef, 'left', left + 'px');
        this.renderer.setElementStyle(modalRef, 'right', 'auto');
        this.renderer.setElementStyle(modalRef, 'bottom', 'auto');
        this.renderer.setElementStyle(modalRef, 'margin', '0');
    }
}