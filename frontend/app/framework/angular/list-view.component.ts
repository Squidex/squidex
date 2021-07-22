/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, HostBinding, Input, Renderer2, ViewChild } from '@angular/core';
import { fadeAnimation, StatefulComponent } from '@app/framework/internal';

interface State {
    // True when loading.
    isLoading: boolean;
}

@Component({
    selector: 'sqx-list-view',
    styleUrls: ['./list-view.component.scss'],
    templateUrl: './list-view.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.Default,
})
export class ListViewComponent extends StatefulComponent<State> implements AfterViewInit {
    private timer: any;

    @ViewChild('headerElement', { static: false })
    public headerElement: ElementRef<ParentNode>;

    @ViewChild('footerElement', { static: false })
    public footerElement: ElementRef<ParentNode>;

    @ViewChild('contentElement', { static: false })
    public contentElement: ElementRef<ParentNode>;

    @Input() @HostBinding('class.overflow')
    public overflow?: boolean | null;

    @Input()
    public syncedHeader?: boolean | null;

    @Input()
    public innerWidth = '100%';

    @Input()
    public table?: boolean | null;

    @Input()
    public isLoaded: boolean | undefined | null = true;

    @Input()
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

    constructor(changeDetector: ChangeDetectorRef,
        private readonly renderer: Renderer2,
    ) {
        super(changeDetector, {
            isLoading: false,
        });
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
