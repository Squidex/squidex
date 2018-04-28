/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, HostListener } from '@angular/core';

import { PanelComponent } from './panel.component';

@Directive({
    selector: '[sqxPanelContainer]'
})
export class PanelContainerDirective implements AfterViewInit {
    private readonly panels: PanelComponent[] = [];
    private containerWidth = 0;

    constructor(
        private readonly element: ElementRef
    ) {
    }

    @HostListener('window:resize')
    public onResize() {
        this.invalidate(true);
    }

    public ngAfterViewInit() {
        this.invalidate(true);
    }

    public push(panel: PanelComponent) {
        this.panels.push(panel);

        this.invalidate();
    }

    public pop() {
        this.panels.splice(-1, 1);

        this.invalidate();
    }

    public invalidate(resize = false) {
        if (resize) {
            this.containerWidth = this.element.nativeElement.getBoundingClientRect().width;
        }

        const panels = this.panels;

        let currentSize = 0;
        let panelsWidthSpread = 0;

        for (let panel of panels) {
            if (panel.desiredWidth !== '*') {
                const layoutWidth = panel.desiredWidth;

                panel.measure(layoutWidth);

                currentSize += panel.renderWidth;
            } else {
                panelsWidthSpread++;
            }
        }

        for (let panel of panels) {
            if (panel.desiredWidth === '*') {
                const layoutWidth = (this.containerWidth - currentSize) / panelsWidthSpread;

                panel.measure(layoutWidth + 'px');

                currentSize += panel.renderWidth;
            }
        }

        let currentPosition = 0;
        let currentLayer = panels.length * 10;

        for (let panel of panels) {
            panel.arrange(currentPosition + 'px', currentLayer.toString());

            currentPosition += panel.renderWidth;
            currentLayer -= 10;
        }

        const diff = currentPosition - this.containerWidth;

        if (diff > 0) {
            this.element.nativeElement.scrollLeft = diff;
        } else {
            this.element.nativeElement.scrollLeft = 0;
        }
    }
}