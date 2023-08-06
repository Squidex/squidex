/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, Input, OnDestroy, Renderer2 } from '@angular/core';
import { timer } from 'rxjs';
import { AnchorX, AnchorY, computeAnchors, positionModal, PositionRequest, PositionResult, RelativePosition, ResourceOwner, Types } from '@app/framework/internal';

@Directive({
    selector: '[sqxAnchoredTo]',
})
export class ModalPlacementDirective extends ResourceOwner implements AfterViewInit, OnDestroy {
    private readonly request: PositionRequest = {
        adjust: true,
        anchorX: 'right-to-right',
        anchorY: 'top-to-bottom',
    } as any;

    private targetField?: Element;
    private isViewInit = false;
    private previousPosition?: PositionResult;

    @Input('sqxAnchoredTo')
    public set target(value: Element) {
        if (value === this.targetField) {
            return;
        }

        if (this.targetField) {
            this.unsubscribeAll();
        }

        this.targetField = value;

        if (this.targetField) {
            this.listenToElement(this.targetField);
        }
    }

    @Input()
    public scrollX = false;

    @Input()
    public scrollY = false;

    @Input()
    public scrollMargin = 10;

    @Input()
    public offsetAuto = 0;

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
    public position?: RelativePosition;

    @Input()
    public adjustWidth = false;

    @Input()
    public adjustHeight = false;

    @Input()
    public update = true;

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

    public ngOnChanges() {
        this.request.anchorX = this.anchorX;
        this.request.anchorY = this.anchorY;
        this.request.offsetX = this.offsetX;
        this.request.offsetY = this.offsetY;

        if (this.position) {
            const [anchorX, offsetX, anchorY, offsetY] = computeAnchors(this.position, this.offsetAuto);

            this.request.anchorX = anchorX;
            this.request.anchorY = anchorY;

            if (Types.isNumber(offsetX)) {
                this.request.offsetX = offsetX;
            }

            if (Types.isNumber(offsetY)) {
                this.request.offsetY = offsetY;
            }
        }

        this.previousPosition = undefined;
        this.updatePosition();
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

        this.isViewInit = true;
        this.updatePosition();
    }

    private updatePosition() {
        if (!this.isViewInit || !this.targetField?.isConnected) {
            return;
        }

        const modalRef = this.element.nativeElement;
        const modalRect = modalRef.getBoundingClientRect();

        if ((modalRect.width === 0 && !this.adjustWidth) || (modalRect.height === 0 && !this.adjustHeight)) {
            return;
        }

        this.request.targetRect = this.targetField.getBoundingClientRect();

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
