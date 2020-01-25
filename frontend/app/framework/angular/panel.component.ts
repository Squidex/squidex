/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, Renderer2, SimpleChanges, ViewChild } from '@angular/core';

import { slideRightAnimation } from '@app/framework/internal';

import { PanelContainerDirective } from './panel-container.directive';

@Component({
    selector: 'sqx-panel',
    styleUrls: ['./panel.component.scss'],
    templateUrl: './panel.component.html',
    animations: [
        slideRightAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class PanelComponent implements AfterViewInit, OnChanges, OnDestroy, OnInit {
    private styleWidth: string;
    private renderWidthField = 0;
    private isViewInitField = false;

    @Output()
    public close = new EventEmitter();

    @Input()
    public theme = 'light';

    @Input()
    public desiredWidth = '10rem';

    @Input()
    public minWidth?: string;

    @Input()
    public isBlank = false;

    @Input()
    public isFullSize = false;

    @Input()
    public isLazyLoaded = true;

    @Input()
    public scrollX = false;

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

    @Input()
    public grid = false;

    @Input()
    public noPadding = false;

    @ViewChild('panel', { static: false })
    public panel: ElementRef<HTMLElement>;

    public get customClose() {
        return this.close.observers.length > 0;
    }

    public get renderWidth() {
        return this.renderWidthField;
    }

    public get isViewInit() {
        return this.isViewInitField;
    }

    constructor(
        private readonly container: PanelContainerDirective,
        private readonly renderer: Renderer2
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['desiredWidth'] && this.isViewInitField) {
            this.container.invalidate();
        }
    }

    public ngOnDestroy() {
        this.container.pop();
    }

    public ngOnInit() {
        this.container.push(this);
    }

    public ngAfterViewInit() {
        this.isViewInitField = true;

        this.container.invalidate();
    }

    public measure(size: string) {
        if (this.styleWidth !== size && this.isViewInitField) {
            this.styleWidth = size;

            const element = this.panel.nativeElement;

            if (element) {
                this.renderer.setStyle(element, 'width', size);
                this.renderer.setStyle(element, 'minWidth', this.minWidth);

                this.renderWidthField = element.offsetWidth;
            }
        }
    }

    public arrange(left: any, layer: any) {
        if (this.isViewInit) {
            const element = this.panel.nativeElement;

            if (element) {
                this.renderer.setStyle(element, 'top', '0px');
                this.renderer.setStyle(element, 'left', left);
                this.renderer.setStyle(element, 'bottom', '0px');
                this.renderer.setStyle(element, 'position', 'absolute');

                this.renderer.setStyle(element, 'z-index', layer);
            }
        }
    }
}