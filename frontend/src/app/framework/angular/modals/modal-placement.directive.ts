/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { afterNextRender, booleanAttribute, computed, Directive, effect, ElementRef, inject, Injector, input, numberAttribute, Renderer2, untracked } from '@angular/core';
import { autoUpdate, computePosition, flip, Middleware, offset, Placement, shift, size } from '@floating-ui/dom';
import { FloatingPlacement, Types } from '@app/framework/internal';

@Directive({
    selector: '[sqxAnchoredTo]',
})
export class ModalPlacementDirective {
    private readonly renderer = inject(Renderer2);
    private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);
    private readonly injector = inject(Injector);
    private readonly options = computed(() => {
        return {
            target: this.target(),
            scrollX: this.scrollX(),
            scrollY: this.scrollY(),
            scrollMargin: this.scrollMargin(),
            offset: this.offset(),
            spaceX: this.spaceX(),
            spaceY: this.spaceY(),
            position: this.position(),
            adjustWidth: this.adjustWidth(),
            adjustHeight: this.adjustHeight(),
        };
    });

    private isViewInit = false;

    /** The element the modal is anchored to. */
    public readonly target = input<Element | undefined>(undefined, { alias: 'sqxAnchoredTo' });

    /** Defines the horizontal overflow behavior of the modal. */
    public readonly scrollX = input<Scroll>();

    /** Defines the vertical overflow behavior of the modal. */
    public readonly scrollY = input<Scroll>();

    /** The margin to the viewport border in pixels when the modal is scrollable. */
    public readonly scrollMargin = input(10, { transform: numberAttribute });

    /** The distance to the anchor element in pixels. */
    public readonly offset = input(2, { transform: numberAttribute });

    /** The additional horizontal space in pixels when the width is adjusted to the anchor. */
    public readonly spaceX = input(0, { transform: numberAttribute });

    /** The additional vertical space in pixels when the height is adjusted to the anchor. */
    public readonly spaceY = input(0, { transform: numberAttribute });

    /** The placement of the modal relative to the anchor element. */
    public readonly position = input<FloatingPlacement>('bottom-end');

    /** Gives the modal the same width as the anchor element. */
    public readonly adjustWidth = input(false, { transform: booleanAttribute });

    /** Gives the modal the same height as the anchor element. */
    public readonly adjustHeight = input(false, { transform: booleanAttribute });

    constructor() {
        this.renderer.setStyle(this.element.nativeElement, 'visibility', 'hidden');

        effect(onCleanup => {
            const target = this.target();
            if (!target) {
                return;
            }

            const unsubscribe = untracked(() => autoUpdate(target, this.element.nativeElement, () => this.updatePosition()));

            onCleanup(unsubscribe);
        });

        effect(() => {
            this.options();

            afterNextRender(() => {
                this.updatePosition();
            }, { injector: this.injector });
        });

        afterNextRender(() => {
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
        });
    }

    private async updatePosition() {
        const options = this.options();

        if (!this.isViewInit || !options.target?.isConnected) {
            return;
        }

        let placement: Placement;
        let placedInside = false;

        if (Types.isArray(options.position)) {
            placement = options.position[0];
            placedInside = options.position[1] === 'inside';
        } else {
            placement = options.position;
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
            middleware.push(offset(options.offset));
            middleware.push(flip());
            middleware.push(shift());
        }

        middleware.push(size({
            apply: ({ availableWidth, availableHeight, rects }) => {
                const overflowX = parseScroll(options.scrollX);
                const overflowY = parseScroll(options.scrollY);

                const maxWidth =
                    overflowX !== 'none'
                    ? availableWidth > 0 ? `${availableWidth - options.scrollMargin}px` : 'none'
                    : undefined;

                const maxHeight =
                    overflowY !== 'none'
                    ? availableHeight > 0 ? `${availableHeight - options.scrollMargin}px` : 'none'
                    : undefined;

                this.renderer.setStyle(modalRef, 'overflow-x', overflowX);
                this.renderer.setStyle(modalRef, 'overflow-y', overflowY);

                this.renderer.setStyle(modalRef, 'max-width', maxWidth);
                this.renderer.setStyle(modalRef, 'max-height', maxHeight);

                if (options.adjustWidth) {
                    const width = rects.reference.width + 2 * options.spaceX;

                    this.renderer.setStyle(modalRef, 'width', `${width}px`);
                }

                if (options.adjustHeight) {
                    const height = rects.reference.height + 2 * options.spaceY;

                    this.renderer.setStyle(modalRef, 'height', `${height}px`);
                }
            },
        }));

        const computedSize = await computePosition(options.target, modalRef, { middleware, placement, strategy: 'fixed' });

        this.renderer.setStyle(modalRef, 'left', `${computedSize.x}px`);
        this.renderer.setStyle(modalRef, 'top', `${computedSize.y}px`);
        this.renderer.setStyle(modalRef, 'visibility', 'visible');
    }
}

type Scroll = 'hidden' | 'scroll' | 'auto' | 'true' | 'false' | true | undefined;

function parseScroll(source: Scroll): string {
    if (source === 'true' || source === 'auto' || source === true) {
        return 'auto';
    }

    if (!source || source === 'false') {
        return 'none';
    }

    return source;
}
