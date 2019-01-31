/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, EmbeddedViewRef, Input, OnChanges, OnDestroy, Renderer2, SimpleChanges, TemplateRef, ViewContainerRef } from '@angular/core';

import {
    DialogModel,
    ModalModel,
    ResourceOwner,
    Types
} from '@app/framework/internal';

import { RootViewComponent } from './root-view.component';
@Directive({
    selector: '[sqxModalView]'
})
export class ModalViewDirective extends ResourceOwner implements OnChanges, OnDestroy {
    private renderedView: EmbeddedViewRef<any> | null = null;

    @Input('sqxModalView')
    public modalView: DialogModel | ModalModel | any;

    @Input('sqxModalViewOnRoot')
    public placeOnRoot = false;

    @Input('sqxModalViewCloseAuto')
    public closeAuto = true;

    @Input('sqxModalViewCloseAlways')
    public closeAlways = false;

    constructor(
        private readonly templateRef: TemplateRef<any>,
        private readonly renderer: Renderer2,
        private readonly viewContainer: ViewContainerRef,
        private readonly rootView: RootViewComponent
    ) {
        super();
    }

    public ngOnDestroy() {
        super.ngOnDestroy();

        if (Types.is(this.modalView, DialogModel) || Types.is(this.modalView, ModalModel)) {
            this.modalView.hide();
        }
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (!changes['modalView']) {
            return;
        }

        super.ngOnDestroy();

        if (Types.is(this.modalView, DialogModel) || Types.is(this.modalView, ModalModel)) {
            this.takeOver(
                this.modalView.isOpen.subscribe(isOpen => {
                    this.update(isOpen);
                }));
        } else {
            this.update(!!this.modalView);
        }
    }

    private update(isOpen: boolean) {
        if (isOpen === (!!this.renderedView)) {
            return;
        }

        if (isOpen && !this.renderedView) {
            const container = this.getContainer();

            this.renderedView = container.createEmbeddedView(this.templateRef);

            if (this.renderedView.rootNodes[0].style) {
                this.renderer.setStyle(this.renderedView.rootNodes[0], 'display', 'block');
            }

            setTimeout(() => {
                this.startListening();
            });
        } else if (!isOpen && this.renderedView) {
            const container = this.getContainer();
            const containerIndex = container.indexOf(this.renderedView);

            container.remove(containerIndex);

            this.renderedView = null;

            super.ngOnDestroy();
        }
    }

    private getContainer() {
        return this.placeOnRoot ? this.rootView.viewContainer : this.viewContainer;
    }

    private startListening() {
        if (!this.closeAuto) {
            return;
        }

        this.takeOver(
            this.renderer.listen('document', 'click', (event: MouseEvent) => {
                if (!event.target || this.renderedView === null) {
                    return;
                }

                if (this.renderedView.rootNodes.length === 0) {
                    return;
                }

                if (this.closeAlways) {
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
            }));
    }
}