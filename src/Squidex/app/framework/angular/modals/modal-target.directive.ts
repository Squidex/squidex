/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, Input, OnDestroy, Renderer2 } from '@angular/core';
import { timer } from 'rxjs';

import { ResourceOwner } from '@app/framework/internal';
import { positionModal } from '@app/shared';

@Directive({
    selector: '[sqxModalTarget]'
})
export class ModalTargetDirective extends ResourceOwner implements AfterViewInit, OnDestroy {
    private targetElement: any;

    @Input('sqxModalTarget')
    public set target(element: any) {
        if (element !== this.targetElement) {
            this.unsubscribeAll();

            this.targetElement = element;

            if (element) {
                this.subscribe(element);
                this.updatePosition();
            }
        }
    }

    @Input()
    public offset = 2;

    @Input()
    public position = 'bottom-right';

    @Input()
    public autoPosition = true;

    constructor(
        private readonly renderer: Renderer2,
        private readonly element: ElementRef
    ) {
        super();
    }

    private subscribe(element: any) {
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

        this.renderer.setStyle(modalRef, 'position', 'fixed');
        this.renderer.setStyle(modalRef, 'z-index', '1000000');

        this.updatePosition();
    }

    private updatePosition() {
        if (!this.targetElement) {
            return;
        }

        const modalRef = this.element.nativeElement;
        const modalRect = this.element.nativeElement.getBoundingClientRect();

        const targetRect: ClientRect = this.targetElement.getBoundingClientRect();

        let y = 0;
        let x = 0;

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

            const position = positionModal(targetRect, modalRect, this.position, this.offset, this.autoPosition, viewW, viewH);

            x = position.x;
            y = position.y;
        }

        this.renderer.setStyle(modalRef, 'top', `${y}px`);
        this.renderer.setStyle(modalRef, 'left', `${x}px`);
        this.renderer.setStyle(modalRef, 'right', 'auto');
        this.renderer.setStyle(modalRef, 'bottom', 'auto');
        this.renderer.setStyle(modalRef, 'margin', '0');
    }
}