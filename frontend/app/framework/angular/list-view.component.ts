/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Input, Renderer2, ViewChild } from '@angular/core';

import { fadeAnimation } from '@app/framework/internal';

@Component({
    selector: 'sqx-list-view',
    styleUrls: ['./list-view.component.scss'],
    templateUrl: './list-view.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.Default
})
export class ListViewComponent implements AfterViewInit {
    private timer: any;

    @ViewChild('headerElement', { static: false })
    public headerElement: ElementRef<ParentNode>;

    @ViewChild('footerElement', { static: false })
    public footerElement: ElementRef<ParentNode>;

    @ViewChild('contentElement', { static: false })
    public contentElement: ElementRef<ParentNode>;

    @Input()
    public syncedHeader = false;

    @Input()
    public table = false;

    @Input()
    public isLoaded = true;

    @Input()
    public set isLoading(value: boolean) {
        if (value) {
            this.isLoadingValue = value;

            this.changeDetector.markForCheck();

            clearTimeout(this.timer);
        } else {
            this.timer = setTimeout(() => {
                this.isLoadingValue = value;

                this.changeDetector.markForCheck();
            }, 250);
        }
    }

    public isLoadingValue = false;

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        private readonly renderer: Renderer2
    ) {
    }

    public ngAfterViewInit() {
        this.hideWhenEmpty(this.headerElement.nativeElement);
        this.hideWhenEmpty(this.footerElement.nativeElement);
        this.hideWhenEmpty(this.contentElement.nativeElement);
    }

    private hideWhenEmpty(element: any) {
        const isEmpty = element.children.length === 0;

        if (isEmpty) {
            this.renderer.setStyle(element, 'display', 'none');
        }
    }
}