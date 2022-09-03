/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';

@Directive({
    selector: '[sqxIfOnce]',
})
export class IfOnceDirective {
    private hasView = false;

    @Input('sqxIfOnce')
    public set condition(value: boolean) {
        if (value && !this.hasView) {
            this.viewContainer.createEmbeddedView(this.templateRef);

            this.hasView = true;
        }
    }

    constructor(
        private templateRef: TemplateRef<any>,
        private viewContainer: ViewContainerRef,
    ) {
    }
}