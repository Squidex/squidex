/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, EmbeddedViewRef, Input, OnChanges, OnDestroy, OnInit, SimpleChanges, TemplateRef, ViewContainerRef } from '@angular/core';

@Directive({
    selector: '[sqxTemplateWrapper]'
})
export class TemplateWrapperDirective implements OnDestroy, OnInit, OnChanges {
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

    public ngOnDestroy() {
        if (this.view) {
            this.view.destroy();
        }
    }

    public ngOnInit() {
        this.view = this.viewContainer.createEmbeddedView(this.templateRef, {
            '\$implicit': this.item,
            'index': this.index
        });
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (this.view) {
            if (changes.item) {
                this.view.context.$implicit = this.item;
            } else if (changes.index) {
                this.view.context.index = this.index;
            }
        }
    }
}