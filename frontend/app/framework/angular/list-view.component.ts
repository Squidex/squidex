/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, Input, Renderer2, ViewChild } from '@angular/core';

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

    @Input()
    public showHeader = true;

    @Input()
    public showFooter = true;

    @Input()
    public isLoaded = true;

    @Input()
    public set isLoading(value: boolean) {
        if (value) {
            this.isLoadingValue = value;

            clearTimeout(this.timer);
        } else {
            this.timer = setTimeout(() => {
                this.isLoadingValue = value;
            }, 250);
        }
    }

    public isLoadingValue = false;

    constructor(
        private readonly renderer: Renderer2
    ) {
    }

    public ngAfterViewInit() {
        this.hideWhenEmpty(this.headerElement.nativeElement);
        this.hideWhenEmpty(this.footerElement.nativeElement);
    }

    private hideWhenEmpty(element: any) {
        const isEmpty = element.children.length === 0;

        if (isEmpty) {
            this.renderer.setStyle(element, 'display', 'none');
        }
    }
}