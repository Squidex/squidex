/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, Input, OnDestroy, Renderer2 } from '@angular/core';
import { timer } from 'rxjs';
import { AnchorX, AnchorY, computeAnchors, positionModal, PositionRequest, RelativePosition, ResourceOwner } from '@app/framework/internal';

@Directive({
    selector: '[sqxAnchoredTo]',
})
export class ModalPlacementDirective extends ResourceOwner implements AfterViewInit, OnDestroy {
    private targetElement?: Element;
    private isViewInit = false;

    @Input('sqxAnchoredTo')
    public set target(element: Element) {
        if (element !== this.targetElement) {
            this.unsubscribeAll();

            this.targetElement = element;

            if (element) {
                this.listenToElement(element);
            }

            if (this.isViewInit) {
                this.updatePosition();
            }
        }
    }

    @Input()
    public offsetX = 0;

    @Input()
    public offsetY = 2;

    @Input()
    public spaceX = 0;

    @Input()
    public spaceY = 0;

    @Input()
    public anchorX: AnchorX = 'right-to-right';

    @Input()
    public anchorY: AnchorY = 'top-to-bottom';

    @Input()
    public adjustWidth = false;

    @Input()
    public adjustHeight = false;

    @Input()
    public scrollX = false;

    @Input()
    public scrollY = false;

    @Input()
    public scrollMargin = 10;

    @Input()
    public update = true;

    @Input()
    public set position(value: RelativePosition) {
        const [anchorX, anchorY] = computeAnchors(value);

        this.anchorX = anchorX;
        this.anchorY = anchorY;
    }

    constructor(
        private readonly renderer: Renderer2,
        private readonly element: ElementRef<HTMLElement>,
    ) {
        super();

        renderer.setStyle(element.nativeElement, 'visibility', 'hidden');
    }

    private listenToElement(element: any) {
        this.own(
            this.renderer.listen(element, 'resize', () => {
                this.updatePosition();
            }));

        this.own(
            this.renderer.listen(this.element.nativeElement, 'resize', () => {
                this.updatePosition();
            }));

        this.own(timer(100, 100).subscribe(() => this.updatePosition()));
    }

    public ngAfterViewInit() {
        const modalRef = this.element.nativeElement;

        this.renderer.setStyle(modalRef, 'margin', '0');
        this.renderer.setStyle(modalRef, 'position', 'fixed');
        this.renderer.setStyle(modalRef, 'bottom', 'auto');
        this.renderer.setStyle(modalRef, 'right', 'auto');

        const zIndex = window.document.defaultView!.getComputedStyle(modalRef).getPropertyValue('z-index');

        if (!zIndex || zIndex === 'auto') {
            this.renderer.setStyle(modalRef, 'z-index', 10000);
        }

        this.updatePosition();

        this.isViewInit = true;
    }

    private updatePosition() {
        if (!this.targetElement) {
            return;
        }

        const modalRef = this.element.nativeElement;
        const modalRect = this.element.nativeElement.getBoundingClientRect();

        if ((modalRect.width === 0 && !this.adjustWidth) || (modalRect.height === 0 && !this.adjustHeight)) {
            return;
        }

        const targetRect = this.targetElement.getBoundingClientRect();

        if (this.scrollX) {
            modalRect.width = modalRef.scrollWidth;
        }

        if (this.scrollY) {
            modalRect.height = modalRef.scrollHeight;
        }

        const clientHeight = document.documentElement!.clientHeight;
        const clientWidth = document.documentElement!.clientWidth;

        const request: PositionRequest = {
            adjust: this.update,
            anchorX: this.anchorX,
            anchorY: this.anchorY,
            clientHeight,
            clientWidth,
            computeHeight: this.adjustHeight,
            computeWidth: this.adjustWidth,
            modalRect,
            offsetX: this.offsetX,
            offsetY: this.offsetY,
            spaceX: this.spaceX,
            spaceY: this.spaceY,
            targetRect,
        };

        const position = positionModal(request);

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

        if (position.width) {
            this.renderer.setStyle(modalRef, 'width', `${position.width}px`);
        }

        if (position.height) {
            this.renderer.setStyle(modalRef, 'height', `${position.height}px`);
        }

        if (position.x) {
            this.renderer.setStyle(modalRef, 'left', `${position.x}px`);
        }

        if (position.y) {
            this.renderer.setStyle(modalRef, 'top', `${position.y}px`);
        }

        this.renderer.setStyle(modalRef, 'visibility', 'visible');
    }
}
