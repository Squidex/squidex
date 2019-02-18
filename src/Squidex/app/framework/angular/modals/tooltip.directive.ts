/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:directive-selector

import { Directive, ElementRef, Input, OnInit, Renderer2 } from '@angular/core';

import { DialogService, ResourceOwner } from '@app/framework/internal';
import { Tooltip } from '@app/shared';

@Directive({
    selector: '[title]'
})
export class TooltipDirective extends ResourceOwner implements OnInit {
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
        super();
    }

    public ngOnInit() {
        const target = this.element.nativeElement;

        this.own(
            this.renderer.listen(target, 'mouseenter', () => {
                if (this.titleText) {
                    this.dialogs.tooltip(new Tooltip(target, this.titleText, this.titlePosition));
                }
            }));

        this.own(
            this.renderer.listen(this.element.nativeElement, 'mouseleave', () => {
                this.dialogs.tooltip(new Tooltip(target, null, this.titlePosition));
            }));
    }

    private unsetAttribute() {
        try {
            this.renderer.setAttribute(this.element.nativeElement, 'title', '');
        } catch {
            return;
        }
    }
}