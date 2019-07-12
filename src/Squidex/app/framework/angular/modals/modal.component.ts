/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewInit, ChangeDetectorRef, Component, EmbeddedViewRef, Input, OnDestroy, Renderer2, TemplateRef, ViewChild } from '@angular/core';
import { timer } from 'rxjs';

import {
    DialogModel,
    ModalModel,
    positionModal,
    ResourceOwner,
    Types
} from '@app/framework/internal';

import { RootViewComponent } from './root-view.component';

declare type Model = DialogModel | ModalModel | any;

@Component({
    selector: 'sqx-modal',
    template: `
        <ng-template #templatePortalContent>
            <ng-content></ng-content>
        </ng-template>
    `
})
export class ModalComponent implements AfterViewInit, OnDestroy {
    private readonly eventsView = new ResourceOwner();
    private readonly eventsModel = new ResourceOwner();
    private currentTarget: Element | null = null;
    private currentModel: DialogModel | ModalModel | null = null;
    private renderedView: EmbeddedViewRef<any> | null = null;
    private renderRoot: HTMLElement | null = null;
    private isOpen: boolean;

    @Input()
    public target: Element;

    @Input()
    public set model(value: Model) {
        if (this.currentModel !== value) {
            this.currentModel = value;

            this.eventsModel.unsubscribeAll();

            this.subscribeToModel(value);
        }
    }

    @Input()
    public offset = 2;

    @Input()
    public position = 'bottom-right';

    @Input()
    public autoPosition = true;

    @Input()
    public backdrop = true;

    @Input()
    public closeAuto = true;

    @Input()
    public closeAlways = false;

    @ViewChild('templatePortalContent', { static: false })
    public templateRef: TemplateRef<any>;

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        private readonly renderer: Renderer2,
        private readonly rootView: RootViewComponent
    ) {
    }

    public ngAfterViewInit() {
        this.update(this.isOpen);
    }

    public ngOnDestroy() {
        hideModal(this.currentModel);

        this.eventsView.unsubscribeAll();
        this.eventsModel.unsubscribeAll();
    }

    public onClick() {
        if (this.closeAlways) {
            this.model.hide();
        }
    }

    private update(isOpen: boolean) {
        if (!this.templateRef || this.isOpen === isOpen) {
            return;
        }

        this.eventsView.unsubscribeAll();

        if (isOpen) {
            if (!this.renderedView) {
                this.currentTarget = this.target;

                this.renderedView = this.rootView.viewContainer.createEmbeddedView(this.templateRef);
                this.renderRoot = this.renderedView.rootNodes[0];

                this.setupStyles();
                this.subscribeToView();

                this.changeDetector.detectChanges();
            }
        } else {
            if (this.renderedView) {
                this.renderedView.destroy();
                this.renderedView = null;
                this.renderRoot = null;

                this.changeDetector.detectChanges();
            }
        }

        this.isOpen = isOpen;
    }

    private setupStyles() {
        this.renderer.setStyle(this.renderRoot, 'display', 'block');
        this.renderer.setStyle(this.renderRoot, 'right', 'auto');
        this.renderer.setStyle(this.renderRoot, 'bottom', 'auto');
        this.renderer.setStyle(this.renderRoot, 'margin', '0');
        this.renderer.setStyle(this.renderRoot, 'position', 'fixed');
        this.renderer.setStyle(this.renderRoot, 'z-index', '1000000');
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
        if (this.renderRoot) {
            this.eventsView.own(this.renderer.listen(this.renderRoot, 'resize', () => {
                this.updatePosition();
            }));

            if (this.currentTarget) {
                this.eventsView.own(this.renderer.listen(this.currentTarget, 'resize', () => {
                    this.updatePosition();
                }));

                this.eventsView.own(timer(100, 100).subscribe(() => {
                    this.updatePosition();
                }));
            }
        }

        if (this.closeAuto) {
            document.addEventListener('click', this.documentClickListener, true);

            this.eventsView.own(() => {
                document.removeEventListener('click', this.documentClickListener);
            });
        }
    }

    private documentClickListener = (event: MouseEvent) => {
        if (!event.target || this.renderRoot === null) {
            return;
        }

        const model = this.currentModel;

        if (this.closeAlways) {
            setTimeout(() => {
                hideModal(model);
            }, 100);
        } else {
            try {
                const rootBounds = this.renderRoot.getBoundingClientRect();

                if (rootBounds.width > 0 && rootBounds.height > 0) {
                    const clickedInside = this.renderRoot.contains(<Node>event.target);

                    if (!clickedInside && this.model) {
                        this.model.hide();
                    }
                }
            } catch (ex) {
                return;
            }
        }
    }

    private updatePosition() {
        if (!this.renderRoot || !this.currentTarget) {
            return;
        }

        const modalRect = this.renderRoot.getBoundingClientRect();

        if ((modalRect.width === 0 || modalRect.height === 0) && this.position !== 'full') {
            return;
        }

        const targetRect = this.currentTarget.getBoundingClientRect();

        let y = 0;
        let x = 0;

        if (this.position === 'full') {
            x = -this.offset + targetRect.left;
            y = -this.offset + targetRect.top;

            const w = 2 * this.offset + targetRect.width;
            const h = 2 * this.offset + targetRect.height;

            this.renderer.setStyle(this.renderRoot, 'width', `${w}px`);
            this.renderer.setStyle(this.renderRoot, 'height', `${h}px`);
        } else {
            const viewH = document.documentElement!.clientHeight;
            const viewW = document.documentElement!.clientWidth;

            const position = positionModal(targetRect, modalRect, this.position, this.offset, this.autoPosition, viewW, viewH);

            x = position.x;
            y = position.y;
        }

        this.renderer.setStyle(this.renderRoot, 'top', `${y}px`);
        this.renderer.setStyle(this.renderRoot, 'left', `${x}px`);
    }
}

function hideModal(model: Model) {
    if (model && isModalModel(model)) {
        model.hide();
    }
}

function isModalModel(model: Model): model is DialogModel | ModalModel {
    return Types.is(model, DialogModel) || Types.is(model, ModalModel);
}