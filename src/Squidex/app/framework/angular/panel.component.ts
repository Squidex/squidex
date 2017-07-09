/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { AfterViewInit, Component, ElementRef, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';

import { slideRightAnimation } from './animations';

import { PanelContainerDirective } from './panel-container.directive';

@Component({
    selector: 'sqx-panel',
    template: `
        <div [style.width]="panelWidth" [attr.expand]="expand" #panel>
            <div class="panel panel-{{theme}}" [@slideRight]>
                <ng-content></ng-content>
            </div>
        </div>`,
    animations: [
        slideRightAnimation
    ]
})
export class PanelComponent implements AfterViewInit, OnDestroy, OnInit {
    @Input()
    public theme = 'light';

    @Input()
    public panelWidth = '10rem';

    @Input()
    public expand = false;

    @ViewChild('panel')
    public panel: ElementRef;

    constructor(
        private readonly container: PanelContainerDirective
    ) {
    }

    public ngOnDestroy() {
        this.container.pop();
    }

    public ngOnInit() {
        this.container.push(this);
    }

    public ngAfterViewInit() {
        this.container.invalidate();
    }
}