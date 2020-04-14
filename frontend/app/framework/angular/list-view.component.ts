/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, HostBinding, Input, Renderer2, ViewChild } from '@angular/core';
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
    private isLoadingValue = false;

    @ViewChild('headerElement', { static: false })
    public headerElement: ElementRef<ParentNode>;

    @ViewChild('footerElement', { static: false })
    public footerElement: ElementRef<ParentNode>;

    @ViewChild('contentElement', { static: false })
    public contentElement: ElementRef<ParentNode>;

    @Input() @HostBinding('class.overflow')
    public overflow = false;

    @Input()
    public syncedHeader = false;

    @Input()
    public table = false;

    @Input()
    public isLoaded = true;

    @Input()
    public set isLoading(value: boolean) {
        clearTimeout(this.timer);

        if (value) {
            this.isLoadingValue = value;

            this.changeDetector.markForCheck();
        } else {
            this.timer = setTimeout(() => {
                this.isLoadingValue = value;

                this.changeDetector.markForCheck();
            }, 250);
        }
    }

    public get isLoading() {
        return this.isLoadingValue;
    }

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        private readonly renderer: Renderer2
    ) {
    }

    public ngAfterViewInit() {
        this.hideWhenEmpty(this.headerElement);
        this.hideWhenEmpty(this.footerElement);
        this.hideWhenEmpty(this.contentElement);
    }

    private hideWhenEmpty(element: ElementRef) {
        if (element && element.nativeElement) {
            const isEmpty = element.nativeElement.children.length === 0;

            if (isEmpty) {
                this.renderer.setStyle(element.nativeElement, 'display', 'none');
            }
        }
    }
}