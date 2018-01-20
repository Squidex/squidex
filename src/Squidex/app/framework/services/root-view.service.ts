/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { EmbeddedViewRef, Injectable, TemplateRef, ViewContainerRef } from '@angular/core';

@Injectable()
export class RootViewService {
    private rootView: ViewContainerRef;

    public init(view: ViewContainerRef) {
        this.rootView = view;
    }

    public createEmbeddedView<C>(templateRef: TemplateRef<C>, context?: C, index?: number): EmbeddedViewRef<C> {
        return this.rootView.createEmbeddedView(templateRef, context, index);
    }

    public clear() {
        this.rootView.clear();
    }
}