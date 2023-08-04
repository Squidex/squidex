/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable import/no-cycle */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Input, OnDestroy, OnInit, Optional, Renderer2, ViewChild } from '@angular/core';
import { ActivatedRoute, NavigationEnd, QueryParamsHandling, Router } from '@angular/router';
import { concat, defer, filter, map, of } from 'rxjs';
import { LayoutContainerDirective } from './layout-container.directive';

@Component({
    selector: 'sqx-layout',
    styleUrls: ['./layout.component.scss'],
    templateUrl: './layout.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LayoutComponent implements OnInit, OnDestroy, AfterViewInit {
    private widthPrevious?: string;
    private widthToRender = 0;
    private isViewInitField = false;

    @Input()
    public closeQueryParamsHandling: QueryParamsHandling = 'preserve';

    @Input()
    public titleText = '';

    @Input()
    public titleIcon = '';

    @Input()
    public titleCollapsed = '';

    @Input()
    public layout: 'left' | 'main' | 'right' = 'main';

    @Input()
    public width = -1;

    @Input()
    public innerWidth = -1;

    @Input()
    public innerWidthPadding = 3;

    @Input()
    public white = false;

    @Input()
    public overflow = false;

    @Input()
    public hideHeader = false;

    @Input()
    public hideSidebar = false;

    @Input()
    public padding = false;

    @Input()
    public customHeader = false;

    @ViewChild('panel', { static: false })
    public panel!: ElementRef<HTMLElement>;

    public isCollapsed = false;
    public isMinimized = false;

    public get desiredInnerWidth() {
        return this.innerWidth <= 0 ? '100%' : `${this.innerWidth}rem`;
    }

    public get desiredWidth() {
        return this.width <= 0 ? '100%' : `${this.width}rem`;
    }

    public get isViewInit() {
        return this.isViewInitField;
    }

    public get renderWidth() {
        return this.widthToRender;
    }

    public firstChild =
        concat(
            defer(() => of(!!this.route?.firstChild)),
            this.router?.events.pipe(
                filter(event => event instanceof NavigationEnd),
                map(() => {
                    return !!this.route?.firstChild;
                }),
            ) || of({}));

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        private readonly container: LayoutContainerDirective,
        private readonly renderer: Renderer2,
        @Optional() public readonly route?: ActivatedRoute,
        @Optional() public readonly router?: Router,
    ) {
    }

    public ngOnDestroy() {
        this.container.peek();
    }

    public ngOnInit() {
        this.container.push(this);
    }

    public ngAfterViewInit() {
        this.isViewInitField = true;

        this.container.invalidate();
    }

    public computeDesiredWidth(numberOfLayouts: number, availableWidth: number) {
        if (this.layout === 'main') {
            return this.width;
        }

        const isMinimized = availableWidth < 1200 && numberOfLayouts > 1;

        if (isMinimized !== this.isMinimized) {
            this.isCollapsed = !this.isMinimized;
            this.isMinimized = isMinimized;
            this.changeDetector.detectChanges();
        }

        return this.isMinimized || this.isCollapsed ? (this.layout === 'left' ? 3 : 0) : this.width;
    }

    public measure(size: string) {
        if (!this.isViewInitField || this.widthPrevious === size)  {
            return;
        }

        this.widthPrevious = size;

        const element = this.panel.nativeElement;

        if (!element) {
            return;
        }

        this.renderer.setStyle(element, 'width', size);

        if (this.layout === 'main') {
            this.renderer.setStyle(element, 'minWidth', `${this.innerWidth + this.innerWidthPadding}rem`);
        }

        this.widthToRender = element.offsetWidth;
    }

    public arrange(left: any, layer: any) {
        if (!this.isViewInitField) {
            return;
        }

        const element = this.panel.nativeElement;

        if (element) {
            this.renderer.setStyle(element, 'top', '0px');
            this.renderer.setStyle(element, 'left', left);
            this.renderer.setStyle(element, 'bottom', '0px');
            this.renderer.setStyle(element, 'position', 'absolute');
            this.renderer.setStyle(element, 'z-index', layer);
        }
    }

    public toggle() {
        this.setCollapsed(!this.isCollapsed);
    }

    public expand(event: MouseEvent) {
        if ((event.target as any)?.['nodeName'] !== 'DIV') {
            return;
        }

        this.setCollapsed(false);
    }

    private setCollapsed(isCollapsed: boolean) {
        if (this.isCollapsed !== isCollapsed) {
            this.isCollapsed = isCollapsed;
            this.container.invalidate();
        }
    }
}
