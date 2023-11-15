/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, booleanAttribute, Directive, ElementRef, Input, numberAttribute, OnDestroy, Renderer2 } from '@angular/core';
import { autoUpdate, computePosition, flip, Middleware, offset, Placement, shift, size } from '@floating-ui/dom';
import { FloatingPlacement, TypedSimpleChanges, Types } from '@app/framework/internal';

@Directive({
    selector: '[sqxAnchoredTo]',
    standalone: true,
})
export class ModalPlacementDirective implements AfterViewInit, OnDestroy {
    private currentListener?: any;
    private isViewInit = false;

    @Input('sqxAnchoredTo')
    public target?: Element;

    @Input({ transform: booleanAttribute })
    public scrollX = false;

    @Input({ transform: booleanAttribute })
    public scrollY = false;

    @Input({ transform: numberAttribute })
    public scrollMargin = 10;

    @Input({ transform: numberAttribute })
    public offset = 2;

    @Input({ transform: numberAttribute })
    public spaceX = 0;

    @Input({ transform: numberAttribute })
    public spaceY = 0;

    @Input()
    public position: FloatingPlacement = 'bottom-end';

    @Input({ transform: booleanAttribute })
    public adjustWidth = false;

    @Input({ transform: booleanAttribute })
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

    private async updatePosition() {
        if (!this.isViewInit || !this.target?.isConnected) {
            return;
        }

        let placement: Placement;
        let placedInside = false;

        if (Types.isArray(this.position)) {
            placement = this.position[0];
            placedInside = this.position[1] === 'inside';
        } else {
            placement = this.position;
        }

        const modalRef = this.element.nativeElement;
        const middleware: Middleware[] = [];

        if (placedInside) {
            middleware.push(offset(({ rects }) => {
                if (placement.startsWith('top') || placement.startsWith('bottom')) {
                    return -rects.floating.height;
                } else {
                    return -rects.floating.width;
                }
            }));
        } else {
            middleware.push(offset(this.offset));
            middleware.push(flip());
            middleware.push(shift());
        }

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
