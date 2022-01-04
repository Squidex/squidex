/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Directive, EmbeddedViewRef, Input, OnDestroy, Renderer2, TemplateRef, ViewContainerRef } from '@angular/core';
import { DialogModel, ModalModel, ResourceOwner, Types } from '@app/framework/internal';
import { RootViewComponent } from './root-view.component';

declare type Model = DialogModel | ModalModel | any;

@Directive({
    selector: '[sqxModal]',
})
export class ModalDirective implements OnDestroy {
    private readonly eventsView = new ResourceOwner();
    private readonly eventsModel = new ResourceOwner();
    private static backdrop: any;
    private currentModel: DialogModel | ModalModel | null = null;
    private renderedView: EmbeddedViewRef<any> | null = null;
    private renderRoots: ReadonlyArray<HTMLElement> | null = null;
    private isOpen = false;

    @Input('sqxModal')
    public set model(value: Model) {
        if (this.currentModel !== value) {
            this.currentModel = value;

            this.eventsModel.unsubscribeAll();

            this.subscribeToModel(value);
        }
    }

    @Input('sqxModalOnRoot')
    public placeOnRoot = true;

    @Input('sqxModalCloseAuto')
    public closeAuto = true;

    @Input('sqxModalCloseAlways')
    public closeAlways = false;

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        private readonly renderer: Renderer2,
        private readonly rootView: RootViewComponent,
        private readonly templateRef: TemplateRef<any>,
        private readonly viewContainer: ViewContainerRef,
    ) {
    }

    public ngOnDestroy() {
        this.hideModal(this.currentModel);

        this.eventsView.unsubscribeAll();
        this.eventsModel.unsubscribeAll();
    }

    private update(isOpen: boolean) {
        if (!this.templateRef || this.isOpen === isOpen) {
            return;
        }

        this.eventsView.unsubscribeAll();

        if (isOpen) {
            if (!this.renderedView) {
                const container = this.getContainer();

                this.renderedView = container.createEmbeddedView(this.templateRef);
                this.renderRoots = this.renderedView.rootNodes.filter(x => !!x.style);

                this.setupStyles();
                this.subscribeToView();

                this.changeDetector.detectChanges();
            }
        } else if (this.renderedView) {
            this.renderedView.destroy();
            this.renderedView = null;
            this.renderRoots = null;

            remove(this.renderer, ModalDirective.backdrop);

            this.changeDetector.detectChanges();
        }

        this.isOpen = isOpen;
    }

    private getContainer() {
        return this.placeOnRoot ? this.rootView.viewContainer : this.viewContainer;
    }

    private setupStyles() {
        if (this.renderRoots) {
            for (const node of this.renderRoots) {
                this.renderer.setStyle(node, 'display', 'block');
                this.renderer.setStyle(node, 'z-index', 2000);
            }
        }
    }

    private subscribeToModel(value: Model) {
        if (isModel(value)) {
            this.currentModel = value;

            this.eventsModel.own(value.isOpenChanges.subscribe(isOpen => this.update(isOpen)));
        } else {
            this.update(!!value);
        }
    }

    private subscribeToView() {
        if (Types.is(this.currentModel, DialogModel)) {
            return;
        }

        if (this.closeAuto && this.renderRoots && this.renderRoots.length > 0) {
            let backdrop = ModalDirective.backdrop;

            if (!backdrop) {
                backdrop = this.renderer.createElement('div');

                this.renderer.setStyle(backdrop, 'position', 'fixed');
                this.renderer.setStyle(backdrop, 'top', 0);
                this.renderer.setStyle(backdrop, 'left', 0);
                this.renderer.setStyle(backdrop, 'right', 0);
                this.renderer.setStyle(backdrop, 'bottom', 0);
                this.renderer.setStyle(backdrop, 'z-index', 1500);

                ModalDirective.backdrop = backdrop;
            }

            insertBefore(this.renderer, this.renderRoots[0], backdrop);

            this.eventsView.own(this.renderer.listen(backdrop, 'click', this.backdropListener));
        }

        if (this.closeAlways && this.renderRoots) {
            for (const node of this.renderRoots) {
                this.eventsView.own(this.renderer.listen(node, 'click', this.elementListener));
            }
        }
    }

    private elementListener = (event: MouseEvent) => {
        if (this.isClickedInside(event)) {
            this.hideModal(this.currentModel);
        }
    };

    private backdropListener = (event: MouseEvent) => {
        if (!this.isClickedInside(event)) {
            this.hideModal(this.currentModel);
        }
    };

    private isClickedInside(event: MouseEvent) {
        try {
            if (!this.renderRoots) {
                return false;
            }

            for (const node of this.renderRoots) {
                if (node.contains(<Node>event.target)) {
                    return true;
                }
            }

            return false;
        } catch (ex) {
            return false;
        }
    }

    private hideModal(model: Model) {
        if (model && isModel(model)) {
            model.hide();

            this.eventsView.unsubscribeAll();
        }
    }
}

function insertBefore(renderer: Renderer2, refElement: any, element: any) {
    if (element && refElement) {
        const parent = renderer.parentNode(refElement);

        if (parent) {
            renderer.insertBefore(parent, element, refElement);
        }
    }
}

function remove(renderer: Renderer2, element: any) {
    if (element) {
        const parent = renderer.parentNode(element);

        if (parent) {
            renderer.removeChild(parent, element);
        }
    }
}

function isModel(model: Model): model is DialogModel | ModalModel {
    return Types.is(model, DialogModel) || Types.is(model, ModalModel);
}
