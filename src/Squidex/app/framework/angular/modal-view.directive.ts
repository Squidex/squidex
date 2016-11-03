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
export class ModalViewDirective implements Ng2.OnChanges {
    private subscription: any | null;
    private isEnabled = true;

    @Ng2.Input('sqxModalView')
    public modalView: ModalView;
    
    constructor(
        private readonly elementRef: Ng2.ElementRef,
        private readonly renderer: Ng2.Renderer,
    ) {
    }

    @Ng2.HostListener('document:click', ['$event', '$event.target'])
    public clickOutside(event: MouseEvent, targetElement: HTMLElement) {
        if (!targetElement) {
            return;
        }

        const clickedInside = this.elementRef.nativeElement.contains(targetElement);
        
        if (!clickedInside && this.modalView && this.isEnabled) {
            this.modalView.hide();
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
                    if (isOpen) {
                        this.renderer.setElementStyle(this.elementRef.nativeElement, 'display', 'block');
                    } else {
                        this.renderer.setElementStyle(this.elementRef.nativeElement, 'display', 'none');
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