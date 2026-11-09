/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, effect, EmbeddedViewRef, inject, input, numberAttribute, TemplateRef, untracked, ViewContainerRef } from '@angular/core';

@Directive({
    selector: '[sqxTemplateWrapper]',
})
export class TemplateWrapperDirective {
    private readonly viewContainer = inject(ViewContainerRef);
    private view?: EmbeddedViewRef<any>;

    /** The item that is passed to the template as implicit context. */
    public readonly item = input<any>();

    /** The index that is passed to the template. */
    public readonly index = input(0, { transform: numberAttribute });

    /** The additional context that is passed to the template. */
    public readonly context = input<any>();

    /** The template to render. */
    public readonly templateRef = input<TemplateRef<any> | null | undefined>(undefined, { alias: 'sqxTemplateWrapper' });

    constructor() {
        effect(onCleanup => {
            const templateRef = this.templateRef();
            if (!templateRef) {
                return;
            }

            const view = untracked(() => this.viewContainer.createEmbeddedView(templateRef, {
                $implicit: this.item(),
                index: this.index(),
                context: this.context(),
            }));

            this.view = view;

            onCleanup(() => {
                view.destroy();

                if (this.view === view) {
                    this.view = undefined;
                }
            });
        });

        effect(() => {
            const item = this.item();
            const index = this.index();
            const context = this.context();

            const view = this.view;
            if (!view) {
                return;
            }

            view.context.$implicit = item;
            view.context.index = index;
            view.context.context = context;
        });
    }
}
