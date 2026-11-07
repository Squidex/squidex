/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgTemplateOutlet } from '@angular/common';
import { afterNextRender, booleanAttribute, ChangeDetectionStrategy, ChangeDetectorRef, Component, computed, contentChild, DestroyRef, ElementRef, inject, input, numberAttribute, Renderer2, signal, viewChild } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, NavigationEnd, QueryParamsHandling, Router, RouterLink } from '@angular/router';
import { EMPTY, filter, map } from 'rxjs';
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
        NgTemplateOutlet,
        RouterLink,
        StopClickDirective,
        TranslatePipe,
    ],
})
export class LayoutComponent {
    private readonly changeDetector = inject(ChangeDetectorRef);
    private readonly container = inject(LayoutContainerDirective);
    private readonly renderer = inject(Renderer2);
    private readonly router = inject(Router, { optional: true });
    private readonly panel = viewChild.required<ElementRef<HTMLElement>>('panel');
    private widthPrevious?: string;
    private widthToRender = 0;
    private isViewInitField = false;

    /** How the query parameters are handled when the layout is closed. */
    public readonly closeQueryParamsHandling = input<QueryParamsHandling>('preserve');

    /** The title in the header. */
    public readonly titleText = input('');

    /** The icon in front of the title. */
    public readonly titleIcon = input('');

    /** The title to show when the left layout is collapsed. Falls back to the title. */
    public readonly titleCollapsed = input('');

    /** The position of the layout. */
    public readonly layout = input<'left' | 'main' | 'right'>('main');

    /** The width in rem. Negative values to use the remaining space. */
    public readonly width = input(-1, { transform: numberAttribute });

    /** The width of the inner content in rem. Negative values to use the full width. */
    public readonly innerWidth = input(-1, { transform: numberAttribute });

    /** The padding in rem that is added to the inner width to compute the minimum width. */
    public readonly innerWidthPadding = input(3, { transform: numberAttribute });

    /** Renders the content with a white background. */
    public readonly white = input(false, { transform: booleanAttribute });

    /** Makes the content scrollable. */
    public readonly overflow = input(false, { transform: booleanAttribute });

    /** Hides the header with the title and menu. */
    public readonly hideHeader = input(false, { transform: booleanAttribute });

    /** Hides the sidebar of the main layout. */
    public readonly hideSidebar = input(false, { transform: booleanAttribute });

    /** Adds a padding around the content. */
    public readonly padding = input(false, { transform: booleanAttribute });

    /** Indicates that a custom header is rendered. */
    public readonly customHeader = input(false, { transform: booleanAttribute });

    /** The width in rem of the left layout when expanded. */
    public readonly expandedWidth = input(-1, { transform: numberAttribute });

    /** The width in rem of the left layout when collapsed. */
    public readonly collapsedWidth = input(3, { transform: numberAttribute });

    public readonly sidebarMenuTemplate = contentChild(SidebarMenuDirective);

    public readonly route = inject(ActivatedRoute, { optional: true });

    public readonly displayMode = signal<DisplayMode>('Normal');

    public readonly isMinimized = signal(false);

    public readonly desiredInnerWidth = computed(() => {
        const innerWidth = this.innerWidth();
        if (innerWidth > 0) {
            return `${innerWidth}rem`;
        }

        return '100%';
    });

    public readonly desiredWidth = computed(() => {
        const actualWidth = this.actualWidth();
        if (actualWidth >= 0) {
            return `${actualWidth}rem`;
        }

        return '100%';
    });

    public readonly actualWidth = computed(() => {
        const displayMode = this.displayMode();
        const layout = this.layout();

        if (layout === 'left') {
            const expandedWidth = this.expandedWidth();
            if (displayMode === 'Expanded' && expandedWidth > 0) {
                return expandedWidth;
            }

            const collapsedWidth = this.collapsedWidth();
            if (displayMode === 'Collapsed' && collapsedWidth > 0) {
                return collapsedWidth;
            }
        } else if (layout === 'right') {
            if (displayMode === 'Collapsed' || this.isMinimized()) {
                return 0;
            }
        }

        return this.width();
    });

    public readonly hasFirstChild = toSignal(
        this.router?.events.pipe(
            filter(event => event instanceof NavigationEnd),
            map(() => !!this.route?.firstChild)) ?? EMPTY,
        { initialValue: !!this.route?.firstChild });

    public get isViewInit() {
        return this.isViewInitField;
    }

    public get renderWidth() {
        return this.widthToRender;
    }

    constructor() {
        this.container.push(this);

        inject(DestroyRef).onDestroy(() => {
            this.container.peek();
        });

        afterNextRender(() => {
            this.isViewInitField = true;
            this.container.invalidate();
        });
    }

    public computeDesiredWidth(numberOfLayouts: number, availableWidth: number) {
        const layout = this.layout();
        if (layout === 'main') {
            return this.width();
        }

        const isCollapsed = availableWidth < 1200 && numberOfLayouts > 1;
        if (this.isMinimized() !== isCollapsed) {
            this.isMinimized.set(isCollapsed);

            this.changeDetector.detectChanges();
        }

        if (layout === 'left' && isCollapsed) {
            return this.collapsedWidth();
        }

        return this.actualWidth();
    }

    public measure(size: string) {
        if (!this.isViewInitField || this.widthPrevious === size) {
            return;
        }

        this.widthPrevious = size;

        const element = this.panel().nativeElement;
        if (!element) {
            return;
        }

        this.renderer.setStyle(element, 'width', size);

        if (this.layout() === 'main') {
            this.renderer.setStyle(element, 'minWidth', `${this.innerWidth() + this.innerWidthPadding()}rem`);
        }

        this.widthToRender = element.offsetWidth;
    }

    public arrange(left: any, layer: any) {
        if (!this.isViewInitField) {
            return;
        }

        const element = this.panel().nativeElement;
        if (!element) {
            return;
        }

        this.renderer.setStyle(element, 'top', '0px');
        this.renderer.setStyle(element, 'left', left);
        this.renderer.setStyle(element, 'bottom', '0px');
        this.renderer.setStyle(element, 'position', 'absolute');
        this.renderer.setStyle(element, 'z-index', layer);
    }

    public toggle() {
        const displayMode = this.displayMode();

        if (displayMode === 'Collapsed') {
            if (this.expandedWidth() > 0) {
                this.setDisplayMode('Expanded');
            } else {
                this.setDisplayMode('Normal');
            }
        } else if (displayMode === 'Expanded') {
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
        if (this.displayMode() !== mode) {
            this.displayMode.set(mode);
            this.container.invalidate();
        }
    }
}
