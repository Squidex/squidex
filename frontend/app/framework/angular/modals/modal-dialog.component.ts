/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, Component, ElementRef, EventEmitter, Input, Output, Renderer2, ViewChild } from '@angular/core';

import { fadeAnimation } from '@app/framework/internal';

@Component({
    selector: 'sqx-modal-dialog',
    styleUrls: ['./modal-dialog.component.scss'],
    templateUrl: './modal-dialog.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.Default
})
export class ModalDialogComponent implements AfterViewInit {
    @Output()
    public close = new EventEmitter();

    @Input()
    public showClose = true;

    @Input()
    public showHeader = true;

    @Input()
    public showFooter = true;

    @Input()
    public showTabs = true;

    @Input()
    public large = false;

    @Input()
    public flexBody = false;

    @Input()
    public fullHeight = false;

    @ViewChild('tabsElement', { static: false })
    public tabsElement: ElementRef<ParentNode>;

    @ViewChild('footerElement', { static: false })
    public footerElement: ElementRef<ParentNode>;

    constructor(
        private readonly renderer: Renderer2
    ) {
    }

    public ngAfterViewInit() {
        this.hideWhenEmpty(this.tabsElement);
        this.hideParentWhenEmpty(this.footerElement);
    }

    private hideWhenEmpty(element: ElementRef) {
        if (element && element.nativeElement) {
            const isEmpty = element.nativeElement.children.length === 0;

            if (isEmpty) {
                this.renderer.setStyle(element.nativeElement, 'display', 'none');
            }
        }
    }

    private hideParentWhenEmpty(element: ElementRef) {
        if (element && element.nativeElement) {
            const isEmpty = element.nativeElement.children.length === 0;

            if (isEmpty) {
                this.renderer.setStyle(element.nativeElement.parentNode, 'display', 'none');
            }
        }
    }
}