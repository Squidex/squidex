/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { DragService } from './../services/drag.service';
import { Vec2 } from './../utils/vec2';

@Ng2.Directive({
    selector: '[gpDragModel]'
})
export class DragModelDirective {
    private startOffset: Vec2;
    private startPosition: Vec2;
    private mouseMoveSubscription: Function | null;
    private mouseUpSubscription: Function | null;
    private clonedElement: HTMLElement | null;

    @Ng2.Input('gpDragModel')
    public model: any;

    constructor(
        private readonly element: Ng2.ElementRef,
        private readonly renderer: Ng2.Renderer,
        private readonly dragService: DragService
    ) {
    }

    @Ng2.HostListener('mousedown', ['$event'])
    public onMouseDown(event: MouseEvent) {
        this.startOffset = new Vec2(event.offsetX, event.offsetY);
        this.startPosition = new Vec2(event.clientX, event.clientY);

        this.mouseMoveSubscription =
            this.renderer.listenGlobal('window', 'mousemove', (e: MouseEvent) => {
                this.onMouseMove(e);
            });

        this.mouseUpSubscription =
            this.renderer.listenGlobal('window', 'mouseup', (e: MouseEvent) => {
                this.onMouseUp(e);
            });

        this.stopEvent(event);
    }

    private onMouseMove(event: MouseEvent) {
        const position = new Vec2(event.clientX, event.clientY);

        if (!this.clonedElement && position.sub(this.startPosition).lengtSquared > 100) {
            this.clonedElement = this.element.nativeElement.cloneNode(true);

            this.clonedElement!.style.position = 'fixed';
            this.clonedElement!.style.zIndex = '10000';

            document.body.appendChild(this.clonedElement!);
        }

        if (this.clonedElement) {
            const elementPosition = position.sub(this.startOffset);

            this.clonedElement.style.left = elementPosition.x + 'px';
            this.clonedElement.style.top = elementPosition.y + 'px';
        }

        this.stopEvent(event);
    }

    private onMouseUp(event: MouseEvent) {
        if (this.clonedElement) {
            this.clonedElement.remove();
        }

        let dropCandidate: Element | null = document.elementFromPoint(event.clientX, event.clientY);

        while (dropCandidate && (dropCandidate.classList && !dropCandidate.classList.contains('sqx-drop'))) {
            dropCandidate = dropCandidate.parentNode as Element;
        }

        if (dropCandidate && dropCandidate.id) {
            const position = this.getRelativeCoordinates(event, dropCandidate).sub(this.startOffset).round();

            this.dragService.emitDrop({ position, model: this.model, dropTarget: dropCandidate.id });
        }

        if (this.mouseMoveSubscription) {
            this.mouseMoveSubscription();
            this.mouseMoveSubscription = null;
        }

        if (this.mouseUpSubscription) {
            this.mouseUpSubscription();
            this.mouseUpSubscription = null;
        }

        this.clonedElement = null;

        this.stopEvent(event);
    }

    private stopEvent(event: Event) {
        event.preventDefault();
        event.stopPropagation();
    }

    private getRelativeCoordinates(e: any, container: any): Vec2 {
        const rect = container.getBoundingClientRect();

        const x = !!e.touches ? e.touches[0].pageX : e.pageX;
        const y = !!e.touches ? e.touches[0].pageY : e.pageY;

        return new Vec2(x - rect.left, y - rect.top);
    }
}