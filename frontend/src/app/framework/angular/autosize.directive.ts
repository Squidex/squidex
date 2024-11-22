/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { AfterViewInit, Directive, DoCheck, ElementRef, HostBinding, HostListener, Input, numberAttribute, Renderer2 } from '@angular/core';

@Directive({
    standalone: true,
    selector: 'textarea[sqxAutosize]',
})
export class AutosizeDirective implements AfterViewInit, DoCheck {
    @HostBinding('style.overflow')
    public overflow = 'hidden';

    @Input({ transform: numberAttribute })
    @HostBinding('rows')
    public rows = 1;

    constructor(
        private readonly element: ElementRef<HTMLTextAreaElement>,
        private readonly renderer: Renderer2,
    ) {
    }

    public ngAfterViewInit() {
        this.resize();
    }

    public ngDoCheck() {
        this.resize();
    }

    @HostListener('input')
    private resize() {
        const textarea = this.element.nativeElement as HTMLTextAreaElement;

        // Calculate border height which is not included in scrollHeight
        const borderHeight = textarea.offsetHeight - textarea.clientHeight;

        this.setHeight(textarea, 'auto');
        this.setHeight(textarea, `${textarea.scrollHeight + borderHeight}px`);
    }

    private setHeight(element: HTMLTextAreaElement, value: string) {
        this.renderer.setStyle(element, 'height', value);
    }
}