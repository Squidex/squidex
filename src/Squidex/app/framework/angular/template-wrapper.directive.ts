/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, Input, OnDestroy, OnInit, OnChanges, TemplateRef, ViewContainerRef, EmbeddedViewRef } from '@angular/core';

@Directive({
    selector: '[sqxTemplateWrapper]'
})
export class TemplateWrapper implements OnInit, OnDestroy, OnChanges {
    @Input()
    public item: any;

    @Input()
    public index: number;

    @Input('sqxTemplateWrapper')
    public templateRef: TemplateRef<any>;

    public view: EmbeddedViewRef<any>;

    public constructor(
        private viewContainer: ViewContainerRef
    ) {
    }

    public ngOnChanges() {
        if (this.view) {
            this.view.context.$implicit = this.item;
            this.view.context.index = this.index;
        }
    }

    public ngOnInit() {
        this.view = this.viewContainer.createEmbeddedView(this.templateRef, {
            '\$implicit': this.item,
            'index': this.index
        });
    }

    public ngOnDestroy() {
        this.view.destroy();
    }
}