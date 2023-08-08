/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, Input, OnDestroy, Renderer2 } from '@angular/core';
import { autoUpdate, computePosition, flip, Middleware, offset, shift, size } from '@floating-ui/dom';
import { FloatingPlacement, TypedSimpleChanges, Types } from '@app/framework/internal';

@Directive({
    selector: '[sqxAnchoredTo]',
})
export class ModalPlacementDirective implements AfterViewInit, OnDestroy {
    private currentListener?: any;
    private isViewInit = false;

    @Input('sqxAnchoredTo')
    public target?: Element;

    @Input()
    public scrollX = false;

    @Input()
    public scrollY = false;

    @Input()
    public scrollMargin = 10;

    @Input()
    public offset = 2;

    @Input()
    public spaceX = 0;

    @Input()
    public spaceY = 0;

    @Input()
    public position: FloatingPlacement = 'bottom-end';

    @Input()
    public adjustWidth = false;

    @Input()
    public adjustHeight = false;

    constructor(
        private readonly renderer: Renderer2,
        private readonly element: ElementRef<HTMLElement>,
    ) {
        renderer.setStyle(element.nativeElement, 'visibility', 'hidden');
    }

    public ngOnDestroy() {
        this.unsubscribe();
    }

    public ngOnChanges(changes: TypedSimpleChanges<ModalPlacementDirective>) {
        if (changes.target?.previousValue) {
            this.unsubscribe();
        }

        if (changes.target?.currentValue) {
            this.subscribe(changes.target?.currentValue);
        }

        setTimeout(() => {
            this.updatePosition();
        });
    }

    public ngAfterViewInit() {
        const modalRef = this.element.nativeElement;

        this.renderer.setStyle(modalRef, 'margin-top', '0');
        this.renderer.setStyle(modalRef, 'margin-left', '0');
        this.renderer.setStyle(modalRef, 'position', 'fixed');

        const zIndex = window.document.defaultView!.getComputedStyle(modalRef).getPropertyValue('z-index');

        if (!zIndex || zIndex === 'auto') {
            this.renderer.setStyle(modalRef, 'z-index', 10000);
        }

        this.isViewInit = true;
        this.updatePosition();
    }

    private unsubscribe() {
        this.currentListener?.();
    }

    private subscribe(element: Element) {
        this.currentListener = autoUpdate(element, this.element.nativeElement, () => this.updatePosition());
    }

    /*
    private updatePosition() {
        if (!this.isViewInit || !this.target?.isConnected) {
            return;
        }

        const modalRef = this.element.nativeElement;
        const modalRect = modalRef.getBoundingClientRect();

        if ((modalRect.width === 0 && !this.adjustWidth) || (modalRect.height === 0 && !this.adjustHeight)) {
            return;
        }

        this.request.targetRect = this.target.getBoundingClientRect();

        if (this.scrollX) {
            modalRect.width = modalRef.scrollWidth;
        }

        if (this.scrollY) {
            modalRect.height = modalRef.scrollHeight;
        }

        this.request.clientWidth = document.documentElement!.clientWidth;
        this.request.clientHeight = document.documentElement!.clientHeight;
        this.request.modalRect = modalRect;

        const position = positionModal(this.request);

        if (Types.equals(position, this.previousPosition)) {
            return;
        }

        this.previousPosition = position;

        if (this.scrollX) {
            const maxWidth = position.maxWidth > 0 ? `${position.maxWidth - this.scrollMargin}px` : 'none';

            this.renderer.setStyle(modalRef, 'overflow-x', 'auto');
            this.renderer.setStyle(modalRef, 'overflow-y', 'none');
            this.renderer.setStyle(modalRef, 'max-width', maxWidth);
        }

        if (this.scrollY) {
            const maxHeight = position.maxHeight > 0 ? `${position.maxHeight - this.scrollMargin}px` : 'none';

            this.renderer.setStyle(modalRef, 'overflow-x', 'none');
            this.renderer.setStyle(modalRef, 'overflow-y', 'auto');
            this.renderer.setStyle(modalRef, 'max-height', maxHeight);
        }

        if (Types.isNumber(position.width)) {
            this.renderer.setStyle(modalRef, 'width', `${position.width}px`);
        }

        if (Types.isNumber(position.height)) {
            this.renderer.setStyle(modalRef, 'height', `${position.height}px`);
        }

        if (Types.isNumber(position.x)) {
            this.renderer.setStyle(modalRef, 'left', `${position.x}px`);
        }

        if (Types.isNumber(position.y)) {
            this.renderer.setStyle(modalRef, 'left', `${position.x}px`);
            this.renderer.setStyle(modalRef, 'top', `${position.y}px`);
        }

        this.renderer.setStyle(modalRef, 'visibility', 'visible');
    }*/

    private async updatePosition() {
        if (!this.isViewInit || !this.target?.isConnected) {
            return;
        }

        const isInside = Types.isArray(this.position) && this.position[1] === 'inside';

        const placement = Types.isArray(this.position) ? this.position[0] : this.position;

        const modalRef = this.element.nativeElement;
        const middleware: Middleware[] = [];

        if (isInside) {
            middleware.push(offset(({ rects }) => {
                if (placement.startsWith('top') || placement.startsWith('bottom')) {
                    return -rects.floating.height;
                } else {
                    return -rects.floating.width;
                }
            }));
        } else if (this.offset !== 0) {
            middleware.push(offset(this.offset));
        }

        middleware.push(flip());
        middleware.push(shift());

        middleware.push(size({
            apply: ({ availableWidth, availableHeight, rects }) => {
                if (this.scrollX) {
                    const maxWidth = availableWidth > 0 ? `${availableWidth - this.scrollMargin}px` : 'none';

                    this.renderer.setStyle(modalRef, 'overflow-x', 'auto');
                    this.renderer.setStyle(modalRef, 'overflow-y', 'none');
                    this.renderer.setStyle(modalRef, 'max-width', maxWidth);
                }

                if (this.scrollY) {
                    const maxHeight = availableHeight > 0 ? `${availableHeight - this.scrollMargin}px` : 'none';

                    this.renderer.setStyle(modalRef, 'overflow-x', 'none');
                    this.renderer.setStyle(modalRef, 'overflow-y', 'auto');
                    this.renderer.setStyle(modalRef, 'max-height', maxHeight);
                }

                if (this.adjustWidth) {
                    const width = rects.reference.width + 2 * this.spaceX;

                    this.renderer.setStyle(modalRef, 'width', `${width}px`);
                }

                if (this.adjustHeight) {
                    const height = rects.reference.height + 2 * this.spaceY;

                    this.renderer.setStyle(modalRef, 'width', `${height}px`);
                }
            },
        }));


        const computedSize = await computePosition(this.target, modalRef, { middleware, placement, strategy: 'fixed' });

        this.renderer.setStyle(modalRef, 'left', `${computedSize.x}px`);
        this.renderer.setStyle(modalRef, 'top', `${computedSize.y}px`);
        this.renderer.setStyle(modalRef, 'visibility', 'visible');
    }
}