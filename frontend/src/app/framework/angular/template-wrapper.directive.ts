/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, EmbeddedViewRef, Input, numberAttribute, OnDestroy, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { TypedSimpleChanges } from '@app/framework/internal';

@Directive({
    selector: '[sqxTemplateWrapper]',
    standalone: true,
})
export class TemplateWrapperDirective implements OnDestroy, OnInit {
    @Input()
    public item: any;

    @Input({ transform: numberAttribute })
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

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
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
