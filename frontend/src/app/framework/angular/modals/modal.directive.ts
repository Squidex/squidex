/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/no-input-rename */

import { booleanAttribute, ChangeDetectorRef, DestroyRef, Directive, effect, EmbeddedViewRef, inject, input, Renderer2, RendererFactory2, TemplateRef, untracked, ViewContainerRef } from '@angular/core';
import { DialogModel, ModalModel, ModalService, OpenModal, Types } from '@app/framework/internal';
import { RootViewComponent } from './root-view.component';

declare type Model = DialogModel | ModalModel | any;

@Directive({
    selector: '[sqxModal]',
})
export class ModalDirective<T = unknown> implements OpenModal {
    private static backdrop: any;

    private readonly backdropListener = (event: MouseEvent) => {
        if (!this.isClickedInside(event)) {
            this.hideModal(this.model());
        }
    };

    private readonly changeDetector = inject(ChangeDetectorRef);
    private readonly currentContext: ModalContext<T> = new ModalContext<T>();
    private readonly destroyRef = inject(DestroyRef);

    private readonly elementListener = (event: MouseEvent) => {
        if (this.isClickedInside(event)) {
            this.hideModal(this.model());
        }
    };

    private readonly modalService = inject(ModalService);
    private readonly renderer = inject(Renderer2);
    private readonly rendererFactory = inject(RendererFactory2);
    private readonly rootView = inject(RootViewComponent);
    private readonly templateRef = inject<TemplateRef<ModalContext<T>>>(TemplateRef);
    private readonly viewContainer = inject(ViewContainerRef);
    private isOpen = false;
    private renderedView: EmbeddedViewRef<ModalContext<T>> | null = null;
    private renderRoots: ReadonlyArray<HTMLElement> | null = null;
    private viewListeners: (() => void)[] = [];

    /** The modal or dialog model that controls the visibility, or any value that renders the template when truthy. */
    public readonly model = input<Model>(undefined, { alias: 'sqxModal' });

    /** Renders the modal in the root view instead of in place. */
    public readonly placeOnRoot = input(true, { alias: 'sqxModalOnRoot', transform: booleanAttribute });

    /** Closes the modal when the user clicks outside of it. */
    public readonly closeAuto = input(true, { alias: 'sqxModalCloseAuto', transform: booleanAttribute });

    /** Closes the modal when the user clicks inside of it. */
    public readonly closeAlways = input(false, { alias: 'sqxModalCloseAlways', transform: booleanAttribute });

    /** Treats the modal as a dialog, which is not closed by clicks. */
    public readonly isDialog = input(false, { alias: 'sqxModalIsDialog', transform: booleanAttribute });

    constructor() {
        this.destroyRef.onDestroy(() => {
            this.hideModal(this.model());

            this.modalService.remove(this);

            this.unlistenView();
        });

        effect(onCleanup => {
            const model = this.model();

            untracked(() => {
                if (isModel(model)) {
                    const subscription = model.isOpenChanges.subscribe(isOpen => this.update(isOpen));

                    onCleanup(() => subscription.unsubscribe());
                } else {
                    this.currentContext.$implicit = model;

                    this.update(!!model);
                }
            });
        });
    }

    public contains(element: Element) {
        return this.renderRoots?.some(x => x.contains(element)) === true;
    }

    public hide() {
        this.hideModal(this.model());
    }

    private update(isOpen: boolean) {
        if (!this.templateRef || this.isOpen === isOpen) {
            return;
        }

        this.unlistenView();

        if (isOpen) {
            if (!this.renderedView) {
                const container = this.getContainer();

                this.renderedView = container.createEmbeddedView(this.templateRef, this.currentContext);
                this.renderRoots = this.renderedView.rootNodes.filter(x => !!x.style);

                this.setupStyles();
                this.subscribeToView();

                this.changeDetector.detectChanges();
            }
        } else if (this.renderedView) {
            this.rendererFactory.begin?.();
            try {
                this.renderedView.destroy();
                this.renderedView = null;
                this.renderRoots = null;
            } finally {
                this.rendererFactory.end?.();
            }

            remove(this.renderer, ModalDirective.backdrop);
        }

        this.isOpen = isOpen;

        if (isOpen) {
            this.modalService.add(this);
        } else {
            this.modalService.remove(this);
        }
    }

    private getContainer() {
        return this.placeOnRoot() ? this.rootView.viewContainer() : this.viewContainer;
    }

    private setupStyles() {
        if (this.renderRoots) {
            for (const node of this.renderRoots) {
                this.renderer.setStyle(node, 'display', 'block');
                this.renderer.setStyle(node, 'z-index', 2000);
            }
        }
    }

    private subscribeToView() {
        if (Types.is(this.model(), DialogModel) || this.isDialog()) {
            return;
        }

        if (this.closeAuto() && this.renderRoots && this.renderRoots.length > 0) {
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

            this.viewListeners.push(this.renderer.listen(backdrop, 'click', this.backdropListener));
        }

        if (this.closeAlways() && this.renderRoots) {
            for (const node of this.renderRoots) {
                this.viewListeners.push(this.renderer.listen(node, 'click', this.elementListener));
            }
        }
    }

    private unlistenView() {
        for (const unlisten of this.viewListeners) {
            unlisten();
        }

        this.viewListeners = [];
    }

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
        } catch {
            return false;
        }
    }

    private hideModal(model: Model) {
        if (model && isModel(model)) {
            model.hide();

            this.unlistenView();
        }
    }
}

export class ModalContext<T = unknown> {
    public $implicit: T = null!;
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
