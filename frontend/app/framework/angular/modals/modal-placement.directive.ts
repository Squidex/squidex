/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, Input, OnDestroy, Renderer2 } from '@angular/core';
import { positionModal, ResourceOwner } from '@app/framework/internal';
import { timer } from 'rxjs';

@Directive({
    selector: '[sqxAnchoredTo]',
})
export class ModalPlacementDirective extends ResourceOwner implements AfterViewInit, OnDestroy {
    private targetElement: Element;

    @Input('sqxAnchoredTo')
    public set target(element: Element) {
        if (element !== this.targetElement) {
            this.unsubscribeAll();

            this.targetElement = element;

            if (element) {
                this.listenToElement(element);
                this.updatePosition();
            }
        }
    }

    @Input()
    public offset = 2;

    @Input()
    public position = 'bottom-right';

    @Input()
    public update = true;

    constructor(
        private readonly renderer: Renderer2,
        private readonly element: ElementRef<Element>,
    ) {
        super();
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
    }

    private updatePosition() {
        if (!this.targetElement) {
            return;
        }

        const modalRef = this.element.nativeElement;
        const modalRect = this.element.nativeElement.getBoundingClientRect();

        if ((modalRect.width === 0 || modalRect.height === 0) && this.position !== 'full') {
            return;
        }

        const targetRect = this.targetElement.getBoundingClientRect();

        let y: number;
        let x: number;

        if (this.position === 'full') {
            x = -this.offset + targetRect.left;
            y = -this.offset + targetRect.top;

            const w = 2 * this.offset + targetRect.width;
            const h = 2 * this.offset + targetRect.height;

            this.renderer.setStyle(modalRef, 'width', `${w}px`);
            this.renderer.setStyle(modalRef, 'height', `${h}px`);
        } else {
            const viewH = document.documentElement!.clientHeight;
            const viewW = document.documentElement!.clientWidth;

            const position = positionModal(targetRect, modalRect, this.position, this.offset, this.update, viewW, viewH);

            x = position.x;
            y = position.y;
        }

        this.renderer.setStyle(modalRef, 'top', `${y}px`);
        this.renderer.setStyle(modalRef, 'left', `${x}px`);
    }
}
