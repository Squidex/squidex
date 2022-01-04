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
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.Default,
})
export class ModalDialogComponent implements AfterViewInit {
    @Output()
    public close = new EventEmitter();

    @Input()
    public showClose?: boolean | null = true;

    @Input()
    public showHeader?: boolean | null = true;

    @Input()
    public showFooter?: boolean | null = true;

    @Input()
    public hasTabs?: boolean | null = false;

    @Input()
    public size: 'sm' | 'md' | 'lg' | 'xl' = 'md';

    @Input()
    public flexBody?: boolean | null;

    @Input()
    public fullHeight?: boolean | null;

    @ViewChild('tabsElement', { static: false })
    public tabsElement!: ElementRef<ParentNode>;

    @ViewChild('footerElement', { static: false })
    public footerElement!: ElementRef<ParentNode>;

    constructor(
        private readonly renderer: Renderer2,
    ) {
    }

    public ngAfterViewInit() {
        this.hideWhenEmpty(this.tabsElement);
        this.hideWhenEmpty(this.footerElement);
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
