/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgTemplateOutlet } from '@angular/common';
import { AfterViewInit, booleanAttribute, ChangeDetectionStrategy, Component, ElementRef, HostBinding, Input, Renderer2, ViewChild } from '@angular/core';
import { StatefulComponent } from '@app/framework/internal';
import { CompensateScrollbarDirective } from './compensate-scrollbar.directive';
import { LoaderComponent } from './loader.component';
import { SyncScollingDirective } from './sync-scrolling.directive';

interface State {
    // True when loading.
    isLoading: boolean;
}

@Component({
    standalone: true,
    selector: 'sqx-list-view',
    styleUrls: ['./list-view.component.scss'],
    templateUrl: './list-view.component.html',
    changeDetection: ChangeDetectionStrategy.Default,
    imports: [
        CompensateScrollbarDirective,
        LoaderComponent,
        NgTemplateOutlet,
        SyncScollingDirective,
    ],
})
export class ListViewComponent extends StatefulComponent<State> implements AfterViewInit {
    private timer: any;

    @ViewChild('headerElement', { static: false })
    public headerElement!: ElementRef<ParentNode>;

    @ViewChild('footerElement', { static: false })
    public footerElement!: ElementRef<ParentNode>;

    @ViewChild('contentElement', { static: false })
    public contentElement!: ElementRef<ParentNode>;

    @Input({ transform: booleanAttribute }) @HostBinding('class.overflow')
    public overflow?: boolean | null;

    @Input({ transform: booleanAttribute })
    public syncedHeader?: boolean | null;

    @Input()
    public innerWidth = '100%';

    @Input({ transform: booleanAttribute })
    public table?: boolean | null;

    @Input({ transform: booleanAttribute })
    public tableNoPadding?: boolean | null;

    @Input({ transform: booleanAttribute })
    public noPadding?: boolean | null;

    @Input({ transform: booleanAttribute })
    public isLoaded: boolean | undefined | null = true;

    @Input({ transform: booleanAttribute })
    public set isLoading(value: boolean | undefined | null) {
        clearTimeout(this.timer);

        if (value) {
            this.next({ isLoading: value });
        } else {
            this.timer = setTimeout(() => {
                this.next({ isLoading: !!value });
            }, 250);
        }
    }

    public get isLoading() {
        return this.snapshot.isLoading;
    }

    constructor(
        private readonly renderer: Renderer2,
    ) {
        super({ isLoading: false });
    }

    public ngAfterViewInit() {
        this.hideWhenEmpty(this.headerElement);
        this.hideWhenEmpty(this.footerElement);
    }

    private hideWhenEmpty(element: ElementRef) {
        if (element && element.nativeElement) {
            const isEmpty = element.nativeElement.children[0].children.length === 0;

            if (isEmpty) {
                this.renderer.setStyle(element.nativeElement, 'display', 'none');
            }
        }
    }
}
