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
    private positionField?: RelativePosition;
    private offsetField = 0;
    private offsetXField = 0;
    private offsetYField = 0;
    private anchorXField: AnchorX = 'right-to-right';
    private anchorYField: AnchorY = 'top-to-bottom';
    private isViewInit = false;
    private previousPosition?: PositionResult;

    @Input('sqxAnchoredTo')
    public set target(value: Element) {
        if (value === this.targetField) {
            return;
        }

        this.unsubscribeAll();

        this.targetField = value;

        if (value) {
            this.listenToElement(value);
        }

        if (this.isViewInit) {
            setTimeout(() => {
                this.updatePosition();
            })
        }
    }

    @Input()
    public scrollX = false;

    @Input()
    public scrollY = false;

    @Input()
    public scrollMargin = 10;

    @Input()
    public set spaceX = 0;
    @Input()
    public set spaceY(value: number | undefined) {
        this.request.spaceY = value;
    }

    @Input()
    public set adjustWidth(value: boolean | undefined) {
        this.request.computeWidth = value;
    }

    @Input()
    public set adjustHeight(value: boolean | undefined) {
        this.request.computeHeight = value;
    }

    @Input()
    public set update(value: boolean | undefined) {
        this.request.adjust = value;
    }

    @Input()
    public set offset(value: number) {
        this.offsetField = value;
        this.invalidateRequest();
    }

    @Input()
    public set anchorX(value: AnchorX) {
        this.anchorXField = value;
        this.invalidateRequest();
    }

    @Input()
    public set anchorY(value: AnchorY) {
        this.anchorYField = value;
        this.invalidateRequest();
    }

    @Input()
    public set offsetX(value: number) {
        this.offsetXField = value;
        this.invalidateRequest();
    }

    @Input()
    public set offsetY(value: number) {
        this.offsetYField = value;
        this.invalidateRequest();
    }

    @Input()
    public set position(value: RelativePosition) {
        this.positionField = value;
        this.invalidateRequest();
    }

    constructor(
        private readonly renderer: Renderer2,
        private readonly element: ElementRef<HTMLElement>,
    ) {
        super();

        renderer.setStyle(element.nativeElement, 'visibility', 'hidden');
        this.invalidateRequest();
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

    private invalidateRequest() {
        this.request.anchorX = this.anchorXField;
        this.request.anchorY = this.anchorYField;
        this.request.offsetX = this.offsetXField;
        this.request.offsetY = this.offsetYField;

        if (this.positionField) {
            const [anchorX, offsetX, anchorY, offsetY] = computeAnchors(this.positionField, this.offsetField);

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
        if (!this.targetField || !this.targetField?.isConnected) {
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
