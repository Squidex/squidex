/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, EmbeddedViewRef, Input, OnChanges, OnDestroy, Renderer2, SimpleChanges, TemplateRef, ViewContainerRef } from '@angular/core';
import { Subscription } from 'rxjs';

import { ModalView, Types } from '@app/framework/internal';

import { RootViewComponent } from './root-view.component';

@Directive({
    selector: '[sqxModalView]'
})
export class ModalViewDirective implements OnChanges, OnDestroy {
    private subscription: Subscription | null = null;
    private documentClickListener: Function | null = null;
    private renderedView: EmbeddedViewRef<any> | null = null;

    @Input('sqxModalView')
    public modalView: ModalView | any;

    @Input('sqxModalViewOnRoot')
    public placeOnRoot = false;

    @Input('sqxModalViewCloseAuto')
    public closeAuto = true;

    constructor(
        private readonly templateRef: TemplateRef<any>,
        private readonly renderer: Renderer2,
        private readonly viewContainer: ViewContainerRef,
        private readonly rootView: RootViewComponent
    ) {
    }

    public ngOnDestroy() {
        this.stopListening();

        if (Types.is(this.modalView, ModalView)) {
            this.modalView.hide();
        }
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (!changes['modalView']) {
            return;
        }

        if (this.subscription) {
            this.subscription.unsubscribe();
            this.subscription = null;
        }

        if (Types.is(this.modalView, ModalView)) {
            this.subscription =
                this.modalView.isOpen.subscribe(isOpen => {
                    this.update(isOpen);
                });
        } else {
            this.update(!!this.modalView);
        }
    }

    private update(isOpen: boolean) {
        if (isOpen === (this.renderedView !== null)) {
            return;
        }

        if (isOpen && !this.renderedView) {
            if (this.placeOnRoot) {
                this.renderedView = this.rootView.viewContainer.createEmbeddedView(this.templateRef);
            } else {
                this.renderedView = this.viewContainer.createEmbeddedView(this.templateRef);
            }

            if (this.renderedView.rootNodes[0].style) {
                this.renderer.setStyle(this.renderedView.rootNodes[0], 'display', 'block');
            }

            setTimeout(() => {
                this.startListening();
            });
        } else if (!isOpen && this.renderedView) {
            this.renderedView = null;

            if (this.placeOnRoot) {
                this.rootView.viewContainer.clear();
            } else {
                this.viewContainer.clear();
            }

            this.stopListening();
        }
    }

    private startListening() {
        if (!this.closeAuto) {
            return;
        }

        this.documentClickListener =
            this.renderer.listen('document', 'click', (event: MouseEvent) => {
                if (!event.target || this.renderedView === null) {
                    return;
                }

                if (this.renderedView.rootNodes.length === 0) {
                    return;
                }

                if (this.modalView.closeAlways) {
                    this.modalView.hide();
                } else {
                    try {
                        const rootNode = this.renderedView.rootNodes[0];
                        const rootBounds = rootNode.getBoundingClientRect();

                        if (rootBounds.width > 0 && rootBounds.height > 0) {
                            const clickedInside = rootNode.contains(event.target);

                            if (!clickedInside && this.modalView) {
                                this.modalView.hide();
                            }
                        }
                    } catch (ex) {
                        return;
                    }
                }
            });
    }

    private stopListening() {
        if (this.documentClickListener) {
            this.documentClickListener();
            this.documentClickListener = null;
        }
    }
}