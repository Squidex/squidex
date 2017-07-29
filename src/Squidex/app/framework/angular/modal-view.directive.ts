/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, EmbeddedViewRef, Input, OnChanges, OnDestroy, Renderer, SimpleChanges, TemplateRef, ViewContainerRef } from '@angular/core';
import { Subscription } from 'rxjs';

import { ModalView } from './../utils/modal-view';

import { RootViewService } from './../services/root-view.service';

@Directive({
    selector: '[sqxModalView]'
})
export class ModalViewDirective implements OnChanges, OnDestroy {
    private subscription: Subscription | null;
    private clickHandler: Function | null;
    private renderedView: EmbeddedViewRef<any> | null = null;

    @Input('sqxModalView')
    public modalView: ModalView;

    @Input('sqxModalViewOnRoot')
    public placeOnRoot = false;

    constructor(
        private readonly templateRef: TemplateRef<any>,
        private readonly renderer: Renderer,
        private readonly viewContainer: ViewContainerRef,
        private readonly rootContainer: RootViewService
    ) {
    }

    public ngOnDestroy() {
        this.stopListening();
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (!changes['modalView']) {
            return;
        }

        if (this.subscription) {
            this.subscription.unsubscribe();
            this.subscription = null;
        }

        if (this.modalView) {
            this.subscription = this.modalView.isOpen.subscribe(isOpen => {
                    if (isOpen === (this.renderedView !== null)) {
                        return;
                    }

                    if (isOpen && !this.renderedView) {
                        if (this.placeOnRoot) {
                            this.renderedView = this.rootContainer.createEmbeddedView(this.templateRef);
                        } else {
                            this.renderedView = this.viewContainer.createEmbeddedView(this.templateRef);
                        }
                        this.renderer.setElementStyle(this.renderedView.rootNodes[0], 'display', 'block');

                        setTimeout(() => {
                            this.startListening();
                        });
                    } else if (!isOpen && this.renderedView) {
                        this.renderedView = null;

                        if (this.placeOnRoot) {
                            this.rootContainer.clear();
                        } else {
                            this.viewContainer.clear();
                        }

                        this.stopListening();
                    }
            });
        }
    }

    private startListening() {
        this.clickHandler =
            this.renderer.listenGlobal('document', 'click', (event: MouseEvent) => {
                if (!event.target || this.renderedView === null) {
                    return;
                }

                if (this.renderedView.rootNodes.length === 0) {
                    return;
                }

                if (this.modalView.closeAlways) {
                    this.modalView.hide();
                } else {
                    const rootNode = this.renderedView.rootNodes[0];
                    const rootBounds = rootNode.getBoundingClientRect();

                    if (rootBounds.width > 0 && rootBounds.height > 0) {
                        const clickedInside = rootNode.contains(event.target);

                        if (!clickedInside && this.modalView) {
                            this.modalView.hide();
                        }
                    }
                }
            });
    }

    private stopListening() {
        if (this.clickHandler) {
            this.clickHandler();
            this.clickHandler = null;
        }
    }
}