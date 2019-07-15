/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Directive, EmbeddedViewRef, Input, OnDestroy, Renderer2, TemplateRef, ViewContainerRef } from '@angular/core';
import { timer } from 'rxjs';

import {
    DialogModel,
    ModalModel,
    ResourceOwner,
    Types
} from '@app/framework/internal';

import { RootViewComponent } from './root-view.component';

declare type Model = DialogModel | ModalModel | any;

@Directive({
    selector: '[sqxModal]'
})
export class ModalDirective implements OnDestroy {
    private readonly eventsView = new ResourceOwner();
    private readonly eventsModel = new ResourceOwner();
    private currentModel: DialogModel | ModalModel | null = null;
    private renderedView: EmbeddedViewRef<any> | null = null;
    private renderRoots: HTMLElement[] | null;
    private isOpen: boolean;

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
        private readonly viewContainer: ViewContainerRef
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
                this.renderedView = this.getContainer().createEmbeddedView(this.templateRef);
                this.renderRoots = this.renderedView.rootNodes.filter(x => !!x.style);

                this.setupStyles();
                this.subscribeToView();

                this.changeDetector.detectChanges();
            }
        } else {
            if (this.renderedView) {
                this.renderedView.destroy();
                this.renderedView = null;
                this.renderRoots = null;

                this.changeDetector.detectChanges();
            }
        }

        this.isOpen = isOpen;
    }

    private getContainer() {
        return this.placeOnRoot ? this.rootView.viewContainer : this.viewContainer;
    }

    private setupStyles() {
        if (this.renderRoots) {
            for (let node of this.renderRoots) {
                this.renderer.setStyle(node, 'display', 'block');
            }
        }
    }

    private subscribeToModel(value: Model) {
        if (isModalModel(value)) {
            this.currentModel = value;

            this.eventsModel.own(value.isOpen.subscribe(update => {
                this.update(update);
            }));
        } else {
            this.update(value === true);
        }
    }

    private subscribeToView() {
        if (this.closeAuto) {
            document.addEventListener('mousedown', this.documentClickListener, true);

            this.eventsView.own(() => {
                document.removeEventListener('mousedown', this.documentClickListener, true);
            });
        }

        if (this.closeAlways && this.renderRoots) {
            for (let node of this.renderRoots) {
                this.eventsView.own(this.renderer.listen(node, 'click', this.elementListener));
            }
        }
    }

    private elementListener = (event: MouseEvent) => {
        if (this.isClickedInside(event)) {
            this.hideModal(this.currentModel);
        }
    }

    private documentClickListener = (event: MouseEvent) => {
        const model = this.currentModel;

        if (!this.isClickedInside(event)) {
            this.eventsView.own(timer(100).subscribe(() => {
                this.hideModal(model);
            }));
        }
    }

    private isClickedInside(event: MouseEvent) {
        try {
            if (!this.renderRoots) {
                return false;
            }

            for (let node of this.renderRoots) {
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
        if (model && isModalModel(model)) {
            model.hide();

            this.eventsView.unsubscribeAll();
        }
    }
}

function isModalModel(model: Model): model is DialogModel | ModalModel {
    return Types.is(model, DialogModel) || Types.is(model, ModalModel);
}