/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';

import { fadeAnimation } from './../animations';

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

    @Input()
    public contentClass = '';

    @Output()
    public closed = new EventEmitter();

    @ViewChild('tabsElement')
    public tabsElement: ElementRef;

    @ViewChild('footerElement')
    public footerElement: ElementRef;

    public hasTabs = false;
    public hasFooter = false;

    constructor(
        private readonly changeDetector: ChangeDetectorRef
    ) {
    }

    public ngAfterViewInit() {
        this.hasTabs = this.tabsElement.nativeElement.children.length > 0;
        this.hasFooter = this.footerElement.nativeElement.children.length > 0;

        this.changeDetector.detectChanges();
    }
}