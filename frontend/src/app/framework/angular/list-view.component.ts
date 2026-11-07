/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgTemplateOutlet } from '@angular/common';
import { afterNextRender, booleanAttribute, ChangeDetectionStrategy, Component, effect, ElementRef, input, Renderer2, signal, viewChild, inject } from '@angular/core';
import { CompensateScrollbarDirective } from './compensate-scrollbar.directive';
import { LoaderComponent } from './loader.component';
import { SyncScollingDirective } from './sync-scrolling.directive';

@Component({
    selector: 'sqx-list-view',
    styleUrls: ['./list-view.component.scss'],
    templateUrl: './list-view.component.html',
    changeDetection: ChangeDetectionStrategy.Default,
    host: {
        ['[class.overflow]']: 'overflow()',
    },
    imports: [
        CompensateScrollbarDirective,
        LoaderComponent,
        NgTemplateOutlet,
        SyncScollingDirective,
    ],
})
export class ListViewComponent {
    private readonly headerElement = viewChild<ElementRef<ParentNode>>('headerElement');
    private readonly footerElement = viewChild<ElementRef<ParentNode>>('footerElement');

    /** Lets the content overflow the list instead of scrolling inside. */
    public readonly overflow = input(false, { transform: booleanAttribute });

    /** Syncs the horizontal scroll position of header and content, for wide tables. */
    public readonly syncedHeader = input(false, { transform: booleanAttribute });

    /** The width of the inner header, content and footer. */
    public readonly innerWidth = input('100%');

    /** Renders the content as table. */
    public readonly table = input(false, { transform: booleanAttribute });

    /** Renders the synced header and the table without padding. */
    public readonly tableNoPadding = input(false, { transform: booleanAttribute });

    /** Renders the content without padding. */
    public readonly noPadding = input(false, { transform: booleanAttribute });

    /** Indicates that the items have been loaded. */
    public readonly isLoaded = input(true, { transform: booleanAttribute });

    /** Shows the loading indicator. It is hidden with a short delay to avoid flickering. */
    public readonly isLoading = input(false, { transform: booleanAttribute });

    protected readonly isLoadingVisible = signal(false);

    constructor() {
        const renderer = inject(Renderer2);

        effect(onCleanup => {
            if (this.isLoading()) {
                this.isLoadingVisible.set(true);
                return;
            }

            const timer = setTimeout(() => this.isLoadingVisible.set(false), 250);

            onCleanup(() => clearTimeout(timer));
        });

        afterNextRender(() => {
            hideWhenEmpty(renderer, this.headerElement());
            hideWhenEmpty(renderer, this.footerElement());
        });
    }
}

function hideWhenEmpty(renderer: Renderer2, element: ElementRef<ParentNode> | undefined) {
    if (!element?.nativeElement) {
        return;
    }

    if (element.nativeElement.children[0].children.length === 0) {
        renderer.setStyle(element.nativeElement, 'display', 'none');
    }
}
