/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Component, ElementRef, Input, OnDestroy, Renderer, ViewChild } from '@angular/core';

import { slideRightAnimation } from './animations';

import { PanelService } from './../services/panel.service';

@Component({
    selector: 'sqx-panel',
    template: `
        <div [style.width]="panelWidth" #panel>
            <div class="panel panel-{{theme}}" [@slideRight]>
                <ng-content></ng-content>
            </div>
        </div>`,
    animations: [
        slideRightAnimation
    ]
})
export class PanelComponent implements OnDestroy, AfterViewInit {
    @Input()
    public theme = 'light';

    @Input()
    public panelWidth = '10rem';

    @ViewChild('panel')
    public panel: ElementRef;

    constructor(
        private readonly renderer: Renderer,
        private readonly panels: PanelService
    ) {
    }

    public ngOnDestroy() {
        this.panels.pop(this.panel.nativeElement);
        this.panels.render(this.renderer);
    }
    public ngAfterViewInit() {
        this.panels.render(this.renderer);
    }
    public ngOnInit() {
        this.panels.push(this.panel.nativeElement);
    }
}