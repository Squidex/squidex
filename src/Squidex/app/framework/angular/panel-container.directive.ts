/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Directive, ElementRef, HostListener, OnDestroy, Renderer } from '@angular/core';

import { PanelComponent } from './panel.component';

@Directive({
    selector: '[sqxPanelContainer]'
})
export class PanelContainerDirective implements AfterViewInit, OnDestroy {
    private readonly panels: PanelComponent[] = [];
    private containerWidth = 0;
    private isInit = false;

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer
    ) {
    }

    @HostListener('window:resize')
    public onResize() {
        this.invalidate(true);
    }

    public ngOnDestroy() {
        this.isInit = true;
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

        let currentPosition = 0;
        let currentLayer = this.panels.length * 10;

        const last = this.panels[this.panels.length - 1];

        for (let panel of this.panels) {
            const panelRoot = panel.panel.nativeElement;

            let layoutWidth = '';

            if (panel.desiredWidth === '*' && panel === last) {
                layoutWidth = (this.containerWidth - currentPosition) + 'px';
            } else {
                layoutWidth = panel.desiredWidth;
            }

            this.renderer.setElementStyle(panelRoot, 'top', '0px');
            this.renderer.setElementStyle(panelRoot, 'left', currentPosition + 'px');
            this.renderer.setElementStyle(panelRoot, 'width', layoutWidth);
            this.renderer.setElementStyle(panelRoot, 'bottom', '0px');
            this.renderer.setElementStyle(panelRoot, 'position', 'absolute');
            this.renderer.setElementStyle(panelRoot, 'z-index', currentLayer.toString());

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