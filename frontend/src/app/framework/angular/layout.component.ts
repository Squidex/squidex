/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgTemplateOutlet } from '@angular/common';
import { AfterViewInit, booleanAttribute, ChangeDetectionStrategy, ChangeDetectorRef, Component, ContentChild, ElementRef, Input, numberAttribute, OnDestroy, OnInit, Optional, Renderer2, ViewChild } from '@angular/core';
import { ActivatedRoute, NavigationEnd, QueryParamsHandling, Router, RouterLink } from '@angular/router';
import { concat, defer, filter, map, of } from 'rxjs';
import { LayoutContainerDirective } from './layout-container.directive';
import { TranslatePipe } from './pipes/translate.pipe';
import { StopClickDirective } from './stop-click.directive';
import { SidebarMenuDirective } from './template.directive';

type DisplayMode = 'Normal' | 'Expanded' | 'Collapsed';

@Component({
    selector: 'sqx-layout',
    styleUrls: ['./layout.component.scss'],
    templateUrl: './layout.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        NgTemplateOutlet,
        RouterLink,
        StopClickDirective,
        TranslatePipe,
    ],
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

    @Input({ transform: numberAttribute })
    public width = -1;

    @Input({ transform: numberAttribute })
    public innerWidth = -1;

    @Input({ transform: numberAttribute })
    public innerWidthPadding = 3;

    @Input({ transform: booleanAttribute })
    public white = false;

    @Input({ transform: booleanAttribute })
    public overflow = false;

    @Input({ transform: booleanAttribute })
    public hideHeader = false;

    @Input({ transform: booleanAttribute })
    public hideSidebar = false;

    @Input({ transform: booleanAttribute })
    public padding = false;

    @Input({ transform: booleanAttribute })
    public customHeader = false;

    @Input({ transform: numberAttribute })
    public expandedWidth = -1;

    @Input({ transform: numberAttribute })
    public collapsedWidth = 3;

    @ViewChild('panel', { static: false })
    public panel!: ElementRef<HTMLElement>;

    @ContentChild(SidebarMenuDirective)
    public sidebarMenuTemplate?: SidebarMenuDirective;

    public displayMode: DisplayMode = 'Normal';

    public isMinimized = false;

    public get desiredInnerWidth() {
        if (this.innerWidth > 0) {
          return `${this.innerWidth}rem`;
        }

        return '100%';
    }

    public get desiredWidth() {
        if (this.actualWidth >= 0) {
            return `${this.actualWidth}rem`;
        }

        return '100%';
    }

    public get actualWidth() {
        const { displayMode, layout, expandedWidth, collapsedWidth, width } = this;

        if (layout === 'left') {
            if (displayMode === 'Expanded' && expandedWidth > 0) {
                return expandedWidth
            }

            if (displayMode === 'Collapsed' && collapsedWidth > 0) {
                return collapsedWidth
            }
        } else if (layout === 'right') {
            if (displayMode === 'Collapsed' || this.isMinimized) {
                return 0;
            }
        }

        return width;
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

        const isCollapsed = availableWidth < 1200 && numberOfLayouts > 1;
        if (this.isMinimized !== isCollapsed) {
            this.isMinimized = isCollapsed;
            this.changeDetector.detectChanges();
        }

        if (this.layout === 'left' && isCollapsed) {
            return this.collapsedWidth;
        }

        return this.actualWidth;
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
        if (this.displayMode === 'Collapsed') {
            if (this.expandedWidth > 0) {
                this.setDisplayMode('Expanded');
            } else {
                this.setDisplayMode('Normal')
            }
        } else if (this.displayMode === 'Expanded') {
            this.setDisplayMode('Normal');
        } else {
            this.setDisplayMode('Collapsed');
        }
    }

    public switchToNormalFromDiv(event: MouseEvent) {
        if ((event.target as any)?.['nodeName'] !== 'DIV') {
            return;
        }

        this.setDisplayMode('Normal');
    }

    private setDisplayMode(mode: DisplayMode) {
        if (this.displayMode !== mode) {
            this.displayMode = mode;
            this.container.invalidate();
        }
    }
}
