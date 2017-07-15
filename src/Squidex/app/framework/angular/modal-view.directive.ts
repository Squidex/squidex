/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Directive, EmbeddedViewRef, Input, OnChanges, OnDestroy, OnInit, Renderer, TemplateRef, ViewContainerRef } from '@angular/core';
import { Subscription } from 'rxjs';

import { ModalView } from './../utils/modal-view';

import { RootViewService } from './../services/root-view.service';

@Directive({
    selector: '[sqxModalView]'
})
export class ModalViewDirective implements OnChanges, OnInit, OnDestroy {
    private subscription: Subscription | null;
    private isEnabled = true;
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
        private readonly rootViewService: RootViewService
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
                        if (this.placeOnRoot) {
                            this.renderedView = this.rootViewService.rootView.createEmbeddedView(this.templateRef);
                        } else {
                            this.renderedView = this.viewContainer.createEmbeddedView(this.templateRef);
                        }
                        this.renderer.setElementStyle(this.renderedView.rootNodes[0], 'display', 'block');
                    } else {
                        this.renderedView = null;

                        if (this.placeOnRoot) {
                            this.rootViewService.rootView.clear();
                        } else {
                            this.viewContainer.clear();
                        }
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