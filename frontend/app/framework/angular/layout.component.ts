/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnDestroy, OnInit } from '@angular/core';
import { LayoutContainerDirective } from './layout-container.directive';

@Component({
    selector: 'sqx-layout',
    styleUrls: ['./layout.component.scss'],
    templateUrl: './layout.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class LayoutComponent implements OnInit, OnDestroy, AfterViewInit {
    private parent?: LayoutComponent;

    @Input()
    public titleText: string;

    @Input()
    public titleIcon: string;

    @Input()
    public layout: 'simple' | 'plain' | 'left' | 'main' = 'simple';

    @Input()
    public width: any = '100%';

    @Input()
    public minWidth: string;

    @Input()
    public white = false;

    @Input()
    public overflow = false;

    @Input()
    public hideSidebar = false;

    @Input()
    public padding = false;

    @Input()
    public customHeader: boolean;

    public childSize: any = 'auto';

    public setChildSize(size: any) {
        this.childSize = size;

        if (this.layout === 'main') {
            this.isCollapsed = false;
        }

        this.changeDetector.detectChanges();
    }

    public isCollapsed = false;

    constructor(
        private readonly container: LayoutContainerDirective,
        private readonly changeDetector: ChangeDetectorRef
    ) {
    }

    public ngOnInit() {
        this.parent = this.container.peek();

        this.container.pushLayout(this);
    }

    public ngAfterViewInit() {
        this.parent?.setChildSize(this.width);

        this.container.invalidate();
    }

    public ngOnDestroy() {
        this.container.popLayout();
    }

    public toggle() {
        this.isCollapsed = !this.isCollapsed;
    }
}