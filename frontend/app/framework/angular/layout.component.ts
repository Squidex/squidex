/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable import/no-cycle */

import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ContentChild, Input, OnDestroy, OnInit } from '@angular/core';
import { QueryParamsHandling, RouterOutlet } from '@angular/router';
import { LayoutContainerDirective } from './layout-container.directive';

@Component({
    selector: 'sqx-layout',
    styleUrls: ['./layout.component.scss'],
    templateUrl: './layout.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LayoutComponent implements OnInit, OnDestroy, AfterViewInit {
    private parent?: LayoutComponent;

    @Input()
    public closeQueryParamsHandling: QueryParamsHandling = 'preserve';

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

    @ContentChild(RouterOutlet)
    public routerOutlet: RouterOutlet;

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
        private readonly changeDetector: ChangeDetectorRef,
    ) {
    }

    public get isCollapsedOrEmpty() {
        if (this.isCollapsed) {
            return true;
        }

        if (this.routerOutlet && this.layout === 'main') {
            return !this.routerOutlet.isActivated;
        }

        return false;
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
