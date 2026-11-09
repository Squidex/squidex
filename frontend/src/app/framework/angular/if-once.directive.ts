/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Directive, effect, inject, input, TemplateRef, untracked, ViewContainerRef } from '@angular/core';

@Directive({
    selector: '[sqxIfOnce]',
})
export class IfOnceDirective {
    private readonly templateRef = inject<TemplateRef<any>>(TemplateRef);
    private readonly viewContainer = inject(ViewContainerRef);
    private hasView = false;

    /** Renders the content the first time the condition is true and keeps it afterwards. */
    public readonly condition = input(false, { alias: 'sqxIfOnce', transform: booleanAttribute });

    constructor() {
        effect(() => {
            if (!this.condition() || this.hasView) {
                return;
            }

            untracked(() => this.viewContainer.createEmbeddedView(this.templateRef));

            this.hasView = true;
        });
    }
}
