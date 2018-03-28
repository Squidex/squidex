/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, Component, ElementRef, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';

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
    public showScrollbar = false;

    @Input()
    public showSidebar = false;

    @Input()
    public showClose = true;

    @Input()
    public contentClass = '';

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
        this.renderWidth = this.panel.nativeElement.getBoundingClientRect().width;

        this.container.invalidate();
    }
}