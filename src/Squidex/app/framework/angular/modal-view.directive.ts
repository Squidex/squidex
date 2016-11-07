/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import * as Ng2 from '@angular/core';

import { ModalView } from './../utils/modal-view';

@Ng2.Directive({
    selector: '[sqxModalView]'
})
export class ModalViewDirective implements Ng2.OnChanges, Ng2.OnInit, Ng2.OnDestroy {
    private subscription: any | null;
    private isEnabled = true;
    private clickHandler: Function | null;
    private renderedView: Ng2.EmbeddedViewRef<any> | null;

    @Ng2.Input('sqxModalView')
    public modalView: ModalView;

    constructor(
        private readonly templateRef: Ng2.TemplateRef<any>,
        private readonly renderer: Ng2.Renderer,
        private readonly viewContainer: Ng2.ViewContainerRef
    ) {
    }

    public ngOnInit() {
        this.clickHandler =
            this.renderer.listenGlobal('document', 'click', (event: MouseEvent) => {
                if (!event.target || this.renderedView === null) {
                    return;
                }

                if (this.renderedView.rootNodes.length === 0) {
                    return;
                }

                if (this.isEnabled) {
                    if (this.modalView.closeAlways) {
                        this.modalView.hide();
                    } else {
                        const clickedInside = this.renderedView.rootNodes[0].contains(event.target);

                        if (!clickedInside && this.modalView) {
                            this.modalView.hide();
                        }
                    }
                }
            });
    }

    public ngOnDestroy() {
        if (this.clickHandler) {
            this.clickHandler();
            this.clickHandler = null;
        }
    }

    public ngOnChanges() {
        if (this.subscription) {
            this.subscription.unsubscribe();
            this.subscription = null;
        }

        if (this.modalView) {
            this.subscription = this.modalView.isOpen.subscribe(isOpen => {
                if (this.isEnabled) {
                    if (isOpen === (this.renderedView !== null)) {
                        return;
                    }

                    if (isOpen) {
                        this.renderedView = this.viewContainer.createEmbeddedView(this.templateRef);
                        this.renderer.setElementStyle(this.renderedView.rootNodes[0], 'display', 'block');
                    } else {
                        this.renderedView = null;
                        this.viewContainer.clear();
                    }

                    this.updateEnabled();
                }
            });
        }
    }

    private updateEnabled() {
        this.isEnabled = false;

        setTimeout(() => {
            this.isEnabled = true;
        }, 500);
    }
}