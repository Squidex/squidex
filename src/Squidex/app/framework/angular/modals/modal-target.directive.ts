/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, Input, OnDestroy, OnInit, Renderer2 } from '@angular/core';

const POSITION_TOPLEFT = 'topLeft';
const POSITION_TOPRIGHT = 'topRight';
const POSITION_BOTTOMLEFT = 'bottomLeft';
const POSITION_BOTTOMRIGHT = 'bottomRight';
const POSITION_LEFTTOP = 'leftTop';
const POSITION_LEFTBOTTOM = 'leftBottom';
const POSITION_RIGHTTOP = 'rightTop';
const POSITION_RIGHTBOTTOM = 'rightBottom';
const POSITION_FULL = 'full';

@Directive({
    selector: '[sqxModalTarget]'
})
export class ModalTargetDirective implements AfterViewInit, OnDestroy, OnInit {
    private elementResizeListener: Function;
    private targetResizeListener: Function;
    private renderTimer: any;
    private targetElement: any;

    @Input('sqxModalTarget')
    public target: any;

    @Input()
    public offset = 2;

    @Input()
    public position = POSITION_BOTTOMRIGHT;

    @Input()
    public autoPosition = true;

    constructor(
        private readonly renderer: Renderer2,
        private readonly element: ElementRef
    ) {
    }

    public ngOnDestroy() {
        if (this.targetResizeListener) {
            this.targetResizeListener();
        }

        if (this.elementResizeListener) {
            this.elementResizeListener();
        }

        clearInterval(this.renderTimer);
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

            this.renderTimer =
                setInterval(() => {
                    this.updatePosition();
                }, 100);
        }
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

        const viewportHeight = document.documentElement.clientHeight;
        const viewportWidth = document.documentElement.clientWidth;

        const modalRef = this.element.nativeElement;
        const modalRect = this.element.nativeElement.getBoundingClientRect();

        const targetRect: ClientRect = this.targetElement.getBoundingClientRect();

        const fix = this.autoPosition;

        let t = 0;
        let l = 0;

        switch (this.position) {
            case POSITION_LEFTTOP:
            case POSITION_RIGHTTOP:
                {
                    t = targetRect.top;
                    break;
                }
            case POSITION_LEFTBOTTOM:
            case POSITION_RIGHTBOTTOM:
                {
                    t = targetRect.bottom - modalRect.height;
                    break;
                }
            case POSITION_BOTTOMLEFT:
            case POSITION_BOTTOMRIGHT:
                {
                    t = targetRect.bottom + this.offset;

                    if (fix && t + modalRect.height > viewportHeight) {
                        const candidate = targetRect.top - modalRect.height - this.offset;

                        if (candidate > 0) {
                            t = candidate;
                        }
                    }
                    break;
                }
            case POSITION_TOPLEFT:
            case POSITION_TOPRIGHT:
                {
                    t = targetRect.top - modalRect.height - this.offset;

                    if (fix && t < 0) {
                        const candidate = targetRect.bottom + this.offset;

                        if (candidate + modalRect.height > viewportHeight) {
                            t = candidate;
                        }
                    }
                    break;
                }
        }

        switch (this.position) {
            case POSITION_TOPLEFT:
            case POSITION_BOTTOMLEFT:
                {
                    l = targetRect.left;
                    break;
                }
            case POSITION_TOPRIGHT:
            case POSITION_BOTTOMRIGHT:
                {
                    l = targetRect.right - modalRect.width;
                    break;
                }
            case POSITION_RIGHTTOP:
            case POSITION_RIGHTBOTTOM:
                {
                    l = targetRect.right + this.offset;

                    if (fix && l + modalRect.width > viewportWidth) {
                        const candidate = targetRect.right - modalRect.width - this.offset;

                        if (candidate > 0) {
                            l = candidate;
                        }
                    }
                    break;
                }
            case POSITION_LEFTTOP:
            case POSITION_LEFTBOTTOM:
                {
                    l = targetRect.left - modalRect.width - this.offset;

                    if (this.autoPosition && l < 0) {
                        const candidate = targetRect.right + this.offset;

                        if (candidate + modalRect.width > viewportWidth) {
                            l = candidate;
                        }
                    }
                    break;
                }
        }

        if (this.position === POSITION_FULL) {
            t = targetRect.top - this.offset;
            l = targetRect.left - this.offset;

            const w = targetRect.width + 2 * this.offset;
            const h = targetRect.height + 2 * this.offset;

            this.renderer.setStyle(modalRef, 'width', `${w}px`);
            this.renderer.setStyle(modalRef, 'height', `${h}px`);
        }

        this.renderer.setStyle(modalRef, 'top', `${t}px`);
        this.renderer.setStyle(modalRef, 'left', `${l}px`);
        this.renderer.setStyle(modalRef, 'right', 'auto');
        this.renderer.setStyle(modalRef, 'bottom', 'auto');
        this.renderer.setStyle(modalRef, 'margin', '0');
    }
}