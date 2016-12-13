/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, ElementRef, OnDestroy, OnInit, Renderer } from '@angular/core';

import { PanelService } from './../services/panel.service';

@Directive({
    selector: '.panel'
})
export class PanelDirective implements OnInit, OnDestroy {
    constructor(
        private readonly element: ElementRef,
        private readonly renderer: Renderer,
        private readonly panels: PanelService
    ) {
    }

    public ngOnInit() {
        this.panels.push(this.element.nativeElement, this.renderer);
    }

    public ngOnDestroy() {
        this.panels.pop(this.element.nativeElement, this.renderer);
    }
}