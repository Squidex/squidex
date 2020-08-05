/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: directive-selector

import { Directive, ElementRef, HostListener, Input, Renderer2 } from '@angular/core';
import { DialogService, Tooltip } from '@app/framework/internal';

@Directive({
    selector: '[title]'
})
export class TooltipDirective {
    private titleText: string;

    @Input()
    public titlePosition = 'top-right';

    @Input()
    public set title(value: string) {
        this.titleText = value;

        this.unsetAttribute();
    }

    constructor(
        private readonly dialogs: DialogService,
        private readonly element: ElementRef,
        private readonly renderer: Renderer2
    ) {
    }

    @HostListener('mouseenter')
    public onMouseEnter() {
        if (this.titleText) {
            this.dialogs.tooltip(new Tooltip(this.element.nativeElement, this.titleText, this.titlePosition));
        }
    }

    @HostListener('mouseleave')
    public onMouseLeave() {
        if (this.titleText) {
            this.dialogs.tooltip(new Tooltip(this.element.nativeElement, null, this.titlePosition));
        }
    }

    @HostListener('click')
    public onClick() {
        if (this.titleText) {
            this.dialogs.tooltip(new Tooltip(this.element.nativeElement, null, this.titlePosition));
        }
    }

    private unsetAttribute() {
        try {
            this.renderer.setProperty(this.element.nativeElement, 'title', '');
        } catch (ex) {
            return;
        }
    }
}