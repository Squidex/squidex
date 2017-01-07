/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, ElementRef, HostListener, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import { PanelService } from './../services/panel.service';

@Directive({
    selector: '.panel-container'
})
export class PanelContainerDirective implements OnInit, OnDestroy {
    private subscription: Subscription;
    private panelsSize: number | null = null;

    constructor(
        private readonly element: ElementRef,
        private readonly panels: PanelService
    ) {
    }

    @HostListener('window:resize')
    public onResize() {
        this.resize();
    }

    public ngOnInit() {
        this.subscription =
            this.panels.changed.subscribe(width => {
                this.panelsSize = width;

                this.resize();
            });
    }

    public ngOnDestroy() {
        this.subscription.unsubscribe();
    }

    private resize() {
        if (!this.panelsSize) {
            return;
        }

        const currentWidth = this.element.nativeElement.getBoundingClientRect().width;

        const diff = this.panelsSize - currentWidth;

        if (diff > 0) {
            this.element.nativeElement.scrollLeft = diff;
        } else {
            this.element.nativeElement.scrollLeft = 0;
        }
    }
}