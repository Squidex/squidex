/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, HostListener, Input, OnDestroy, Renderer2 } from '@angular/core';
import { DialogService, Tooltip } from '@app/framework/internal';

@Directive({
    selector: '[title]',
})
export class TooltipDirective implements OnDestroy {
    private titleText: string;
    private timer: any;

    @Input()
    public titlePosition = 'top-right';

    @Input()
    public titleDelay = 1000;

    @Input()
    public set title(value: string) {
        this.titleText = value;

        this.unsetAttribute();
    }

    constructor(
        private readonly dialogs: DialogService,
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
    }

    public ngOnDestroy() {
        this.hide();
    }

    @HostListener('mouseenter')
    public onMouseEnter() {
        this.hide();

        if (this.titleText) {
            if (this.titleDelay > 0) {
                this.timer = setTimeout(() => {
                    this.show();
                }, this.titleDelay);
            } else {
                this.show();
            }
        }
    }

    @HostListener('mouseleave')
    public onMouseLeave() {
        this.hide();
    }

    @HostListener('click')
    public onClick() {
        this.hide();
    }

    private hide() {
        if (this.titleText) {
            this.dialogs.tooltip(new Tooltip(this.element.nativeElement, null, this.titlePosition));
        }

        if (this.timer) {
            clearTimeout(this.timer);
        }
    }

    private show() {
        this.dialogs.tooltip(new Tooltip(this.element.nativeElement, this.titleText, this.titlePosition));
    }

    private unsetAttribute() {
        try {
            this.renderer.setProperty(this.element.nativeElement, 'title', '');

            return true;
        } catch (ex) {
            return false;
        }
    }
}
