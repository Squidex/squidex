/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { PanelService } from './../services/panel.service';

@Ng2.Directive({
    selector: '.panel-container'
})
export class PanelContainerDirective implements Ng2.OnInit, Ng2.OnDestroy {
    private subscription: any;
    private panelsSize: number | null = null;

    constructor(
        private readonly element: Ng2.ElementRef,
        private readonly panels: PanelService
    ) {
    }

    @Ng2.HostListener('window:resize')
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