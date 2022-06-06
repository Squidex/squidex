/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, HostListener, Input, OnDestroy, Renderer2 } from '@angular/core';
import { DialogService, RelativePosition, Tooltip } from '@app/framework/internal';

@Directive({
    selector: '[title]:not(sqx-layout),[shortcut]',
})
export class TooltipDirective implements OnDestroy {
    private titleText: string | undefined | null;
    private timer: any;
    private shortcutTimer: any;

    @Input()
    public shortcutPosition: RelativePosition = 'bottom-left';

    @Input()
    public shortcut?: string | undefined;

    @Input()
    public shortcutDelay = 2000;

    @Input()
    public titlePosition: RelativePosition = 'top-right';

    @Input()
    public titleDelay = 1000;

    @Input()
    public set title(value: string | undefined | null) {
        this.titleText = value;

        this.unsetAttribute();
    }

    private get target() {
        return this.element.nativeElement;
    }

    constructor(
        private readonly dialogs: DialogService,
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
    }

    public ngOnDestroy() {
        this.hide();
        this.hideShortcut();
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
            this.dialogs.tooltip(new Tooltip(this.target, null, this.titlePosition));
        }

        if (this.timer) {
            clearTimeout(this.timer);

            this.timer = undefined;
        }
    }

    private hideShortcut() {
        if (this.shortcut) {
            this.dialogs.tooltip(new Tooltip(this.target, null, this.titlePosition));
        }

        if (this.shortcutTimer) {
            clearTimeout(this.shortcutTimer);

            this.shortcutTimer = undefined;
        }
    }

    private show() {
        this.dialogs.tooltip(new Tooltip(this.target, this.titleText!, this.titlePosition, false, this.shortcut));
    }

    private unsetAttribute() {
        try {
            this.renderer.setProperty(this.target, 'title', '');

            return true;
        } catch (ex) {
            return false;
        }
    }
}
