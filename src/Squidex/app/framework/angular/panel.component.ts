/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Component, ElementRef, Input, OnDestroy, OnInit, Renderer2, ViewChild } from '@angular/core';

import { slideRightAnimation } from './animations';

import { PanelContainerDirective } from './panel-container.directive';

@Component({
    selector: 'sqx-panel',
    styleUrls: ['./panel.component.scss'],
    templateUrl: './panel.component.html',
    animations: [
        slideRightAnimation
    ]
})
export class PanelComponent implements AfterViewInit, OnDestroy, OnInit {
    private styleWidth: string;

    public renderWidth = 0;

    @Input()
    public theme = 'light';

    @Input()
    public desiredWidth = '10rem';

    @Input()
    public isBlank = false;

    @Input()
    public isFullSize = false;

    @Input()
    public isLazyLoaded = true;

    @Input()
    public showScrollbar = false;

    @Input()
    public showSecondHeader = false;

    @Input()
    public showSidebar = false;

    @Input()
    public showClose = true;

    @Input()
    public contentClass = '';

    @Input()
    public sidebarClass = '';

    @ViewChild('panel')
    public panel: ElementRef;

    constructor(
        private readonly container: PanelContainerDirective,
        private readonly renderer: Renderer2
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

    public measure(size: string) {
        if (this.styleWidth !== size) {
            this.styleWidth = size;

            this.renderer.setStyle(this.panel.nativeElement, 'width', size);

            this.renderWidth = this.panel.nativeElement.getBoundingClientRect().width;
        }
    }

    public arrange(left: any, layer: any) {
        this.renderer.setStyle(this.panel.nativeElement, 'top', '0px');
        this.renderer.setStyle(this.panel.nativeElement, 'left', left);
        this.renderer.setStyle(this.panel.nativeElement, 'bottom', '0px');
        this.renderer.setStyle(this.panel.nativeElement, 'position', 'absolute');
        this.renderer.setStyle(this.panel.nativeElement, 'z-index', layer);
    }
}