/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, EmbeddedViewRef, Input, OnChanges, OnDestroy, OnInit, SimpleChanges, TemplateRef, ViewContainerRef } from '@angular/core';

@Directive({
    selector: '[sqxTemplateWrapper]',
})
export class TemplateWrapperDirective implements OnDestroy, OnInit, OnChanges {
    @Input()
    public item: any;

    @Input()
    public index = 0;

    @Input()
    public context: any;

    @Input('sqxTemplateWrapper')
    public templateRef!: TemplateRef<any>;

    public view?: EmbeddedViewRef<any>;

    public constructor(
        private readonly viewContainer: ViewContainerRef,
    ) {
    }

    public ngOnDestroy() {
        if (this.view) {
            this.view.destroy();
        }
    }

    public ngOnInit() {
        const { index, context } = this;

        const data = {
            $implicit: this.item,
            index,
            context,
        };

        this.view = this.viewContainer.createEmbeddedView(this.templateRef, data);
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (this.view) {
            if (changes.item) {
                this.view.context.$implicit = this.item;
            }

            if (changes.index) {
                this.view.context.index = this.index;
            }

            if (changes.context) {
                this.view.context.context = this.context;
            }
        }
    }
}
