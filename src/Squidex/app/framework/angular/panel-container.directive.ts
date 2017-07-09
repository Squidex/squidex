/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Directive, ElementRef, HostListener, OnDestroy, Renderer } from '@angular/core';

import { PanelComponent } from './panel.component';

@Directive({
    selector: '.panel-container'
})
export class PanelContainerDirective implements AfterViewInit, OnDestroy {
    private readonly panels: PanelComponent[] = [];
    private isInit = false;

    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer
    ) {
    }

    @HostListener('window:resize')
    public onResize() {
        this.invalidate();
    }

    public ngAfterViewInit() {
        this.invalidate(true);
    }

    public ngOnDestroy() {
        this.isInit = true;
    }

    public push(panel: PanelComponent) {
        this.panels.push(panel);

        this.invalidate();
    }

    public pop() {
        this.panels.splice(-1, 1);

        this.invalidate();
    }

    public invalidate(force = false) {
        this.isInit = this.isInit || force;

        if (!this.isInit) {
            return;
        }

        const containerWidth = this.element.nativeElement.getBoundingClientRect().width;

        let currentPosition = 0;
        let currentLayer = this.panels.length * 10;

        const last = this.panels[this.panels.length - 1];

        for (let panel of this.panels) {
            const panelRoot = panel.panel.nativeElement;

            let width = panelRoot.getBoundingClientRect().width;

            if (panel.expand && panel === last) {
                width = containerWidth - currentPosition;

                panel.panelWidth = width + 'px';
            }

            this.renderer.setElementStyle(panelRoot, 'top', '0px');
            this.renderer.setElementStyle(panelRoot, 'left', currentPosition + 'px');
            this.renderer.setElementStyle(panelRoot, 'bottom', '0px');
            this.renderer.setElementStyle(panelRoot, 'position', 'absolute');
            this.renderer.setElementStyle(panelRoot, 'z-index', currentLayer.toString());

            currentPosition += width;
            currentLayer -= 10;
        }

        const diff = currentPosition - containerWidth;

        if (diff > 0) {
            this.element.nativeElement.scrollLeft = diff;
        } else {
            this.element.nativeElement.scrollLeft = 0;
        }
    }
}