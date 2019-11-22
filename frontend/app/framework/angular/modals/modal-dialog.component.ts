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
    public fullHeight = false;

    @Input()
    public tabsClass = '';

    @ViewChild('footerElement', { static: false })
    public footerElement: ElementRef<ParentNode>;

    constructor(
        private readonly renderer: Renderer2
    ) {
    }

    public ngAfterViewInit() {
        this.hideWhenEmpty(this.footerElement.nativeElement);
    }

    private hideWhenEmpty(element: any) {
        const isEmpty = element.children.length === 0;

        if (isEmpty) {
            this.renderer.setStyle(element, 'display', 'none');
        }
    }

    public emitClose() {
        this.close.emit();
    }
}