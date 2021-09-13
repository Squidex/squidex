/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable import/no-cycle */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, Input, OnDestroy, OnInit, Renderer2, ViewChild } from '@angular/core';
import { ActivatedRoute, NavigationEnd, QueryParamsHandling, Router } from '@angular/router';
import { filter, map, startWith } from 'rxjs/operators';
import { LayoutContainerDirective } from './layout-container.directive';

@Component({
    selector: 'sqx-layout',
    styleUrls: ['./layout.component.scss'],
    templateUrl: './layout.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LayoutComponent implements OnInit, OnDestroy, AfterViewInit {
    private widthPrevious: string;
    private widthToRender = 0;
    private isViewInitField = false;

    @Input()
    public closeQueryParamsHandling: QueryParamsHandling = 'preserve';

    @Input()
    public titleText: string;

    @Input()
    public titleIcon: string;

    @Input()
    public titleCollapsed: string;

    @Input()
    public layout: 'simple' | 'left' | 'main' = 'simple';

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
    public customHeader: boolean;

    @ViewChild('panel', { static: false })
    public panel: ElementRef<HTMLElement>;

    public get desiredWidth() {
        return this.isCollapsed ? 3 : this.width;
    }

    public get desiredInnerWidth() {
        return this.innerWidth <= 0 ? '100%' : `${this.innerWidth}rem`;
    }

    public get isViewInit() {
        return this.isViewInitField;
    }

    public get renderWidth() {
        return this.widthToRender;
    }

    public isCollapsed = false;

    public firstChild =
        this.router.events.pipe(
            filter(event => event instanceof NavigationEnd),
            map(() => {
                return !!this.route.firstChild;
            }),
            startWith(!!this.route.firstChild),
        );

    constructor(
        private readonly container: LayoutContainerDirective,
        private readonly renderer: Renderer2,
        public readonly route: ActivatedRoute,
        public readonly router: Router,
    ) {
    }

    public ngOnInit() {
        this.container.push(this);
    }

    public ngAfterViewInit() {
        this.isViewInitField = true;

        this.container.invalidate();
    }

    public ngOnDestroy() {
        this.container.peek();
    }

    public measure(size: string) {
        if (this.widthPrevious !== size && this.isViewInitField) {
            this.widthPrevious = size;

            const element = this.panel.nativeElement;

            if (element) {
                this.renderer.setStyle(element, 'width', size);

                if (this.layout === 'main') {
                    this.renderer.setStyle(element, 'minWidth', `${this.innerWidth + this.innerWidthPadding}rem`);
                }

                this.widthToRender = element.offsetWidth;
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

    public toggle() {
        this.isCollapsed = !this.isCollapsed;

        this.container.invalidate();
    }
}
