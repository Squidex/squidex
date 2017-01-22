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
    public theme: string = 'light';

    @Input()
    public panelWidth: string = '10rem';

    @ViewChild('panel')
    public panel: ElementRef;

    constructor(
        private readonly renderer: Renderer,
        private readonly panels: PanelService
    ) {
    }

    public ngOnDestroy() {
        this.panels.pop(this.panel.nativeElement, this.renderer);
    }

    public ngAfterViewInit() {
        this.panels.push(this.panel.nativeElement, this.renderer);
    }
}