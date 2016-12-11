/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { PanelService } from './../services/panel.service';

@Ng2.Directive({
    selector: '.panel'
})
export class PanelDirective implements Ng2.OnInit, Ng2.OnDestroy {
    constructor(
        private readonly element: Ng2.ElementRef,
        private readonly renderer: Ng2.Renderer,
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