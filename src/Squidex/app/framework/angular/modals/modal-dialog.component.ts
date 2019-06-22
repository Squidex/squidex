/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';

import { fadeAnimation, StatefulComponent } from '@app/framework/internal';

interface State {
    hasTabs: boolean;
    hasFooter: boolean;
}

@Component({
    selector: 'sqx-modal-dialog',
    styleUrls: ['./modal-dialog.component.scss'],
    templateUrl: './modal-dialog.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.Default
})
export class ModalDialogComponent extends StatefulComponent<State> implements AfterViewInit {
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
    public close = new EventEmitter();

    @ViewChild('tabsElement', { static: false })
    public tabsElement: ElementRef<ParentNode>;

    @ViewChild('footerElement', { static: false })
    public footerElement: ElementRef<ParentNode>;

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            hasTabs: false,
            hasFooter: false
        });
    }

    public ngAfterViewInit() {
        const hasTabs = this.tabsElement.nativeElement.children.length > 0;
        const hasFooter = this.footerElement.nativeElement.children.length > 0;

        this.next(() => ({ hasTabs, hasFooter }));
    }

    public emitClose() {
        this.close.emit();
    }
}