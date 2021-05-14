/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable import/no-cycle */

import { AfterViewInit, Directive, ElementRef, HostListener, Renderer2 } from '@angular/core';
import { PanelComponent } from './panel.component';

@Directive({
    selector: '[sqxPanelContainer]',
})
export class PanelContainerDirective implements AfterViewInit {
    private readonly panels: PanelComponent[] = [];
    private isViewInit = false;
    private containerWidth = 0;

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
    }

    @HostListener('window:resize')
    public onResize() {
        this.invalidate(true);
    }

    public push(panel: PanelComponent) {
        this.panels.push(panel);
    }

    public ngAfterViewInit() {
        this.isViewInit = true;

        this.invalidate(true);
    }

    public pop() {
        this.panels.splice(-1, 1);

        this.invalidate();
    }

    public invalidate(resize = false) {
        if (!this.isViewInit) {
            return;
        }

        if (resize) {
            this.containerWidth = this.element.nativeElement.offsetWidth;
        }

        const panels = this.panels;

        for (const panel of panels) {
            if (!panel.isViewInit) {
                return;
            }
        }

        let currentSize = 0;
        let panelsWidthSpread = 0;

        for (const panel of panels) {
            if (panel.desiredWidth !== '*') {
                const layoutWidth = panel.desiredWidth;

                panel.measure(layoutWidth);

                currentSize += panel.renderWidth;
            } else {
                panelsWidthSpread++;
            }
        }

        for (const panel of panels) {
            if (panel.desiredWidth === '*') {
                const layoutWidth = (this.containerWidth - currentSize) / panelsWidthSpread;

                panel.measure(`${layoutWidth}px`);

                currentSize += panel.renderWidth;
            }
        }

        let currentPosition = 0;
        let currentLayer = panels.length * 10;

        for (const panel of panels) {
            panel.arrange(`${currentPosition}px`, currentLayer.toString());

            currentPosition += panel.renderWidth;
            currentLayer -= 10;
        }

        const diff = Math.max(0, currentPosition - this.containerWidth);

        this.renderer.setProperty(this.element.nativeElement, 'scrollLeft', diff);
    }
}
