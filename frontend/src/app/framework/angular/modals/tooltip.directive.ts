/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/directive-selector */

import { DestroyRef, Directive, effect, ElementRef, inject, input, numberAttribute, Renderer2 } from '@angular/core';
import { DialogService, FloatingPlacement, Tooltip } from '@app/framework/internal';

@Directive({
    selector: '[title]:not(sqx-layout),[shortcut]',
    host: {
        '(mouseenter)': 'onMouseEnter()',
        '(mouseleave)': 'onMouseLeave()',
        '(click)': 'onClick()',
    },
})
export class TooltipDirective {
    private readonly destroyRef = inject(DestroyRef);
    private readonly dialogs = inject(DialogService);
    private readonly element = inject(ElementRef);
    private readonly renderer = inject(Renderer2);
    private shortcutTimer: any;
    private timer: any;

    /** The placement of the shortcut hint relative to the element. */
    public readonly shortcutPosition = input<FloatingPlacement>('bottom-start');

    /** The keyboard shortcut that is shown in the tooltip. */
    public readonly shortcut = input<string | undefined>();

    /** The delay in milliseconds before the shortcut hint is shown. */
    public readonly shortcutDelay = input(2000, { transform: numberAttribute });

    /** The placement of the tooltip relative to the element. */
    public readonly titlePosition = input<FloatingPlacement>('top-end');

    /** The delay in milliseconds before the tooltip is shown. */
    public readonly titleDelay = input(1000, { transform: numberAttribute });

    /** The text of the tooltip, which replaces the native title. */
    public readonly title = input<string | undefined | null>();

    private get target() {
        return this.element.nativeElement;
    }

    constructor() {
        effect(() => {
            this.title();
            this.unsetAttribute();
        });

        this.destroyRef.onDestroy(() => {
            this.hide();
            this.hideShortcut();
        });
    }

    protected onMouseEnter() {
        this.hide();

        if (this.title()) {
            const delay = this.titleDelay();

            if (delay > 0) {
                this.timer = setTimeout(() => {
                    this.show();
                }, delay);
            } else {
                this.show();
            }
        }
    }

    protected onMouseLeave() {
        this.hide();
    }

    protected onClick() {
        this.hide();
    }

    private hide() {
        if (this.title()) {
            this.dialogs.tooltip(new Tooltip(this.target, null, this.titlePosition()));
        }

        if (this.timer) {
            clearTimeout(this.timer);

            this.timer = undefined;
        }
    }

    private hideShortcut() {
        if (this.shortcut()) {
            this.dialogs.tooltip(new Tooltip(this.target, null, this.titlePosition()));
        }

        if (this.shortcutTimer) {
            clearTimeout(this.shortcutTimer);

            this.shortcutTimer = undefined;
        }
    }

    private show() {
        this.dialogs.tooltip(new Tooltip(this.target, this.title()!, this.titlePosition(), false, this.shortcut()));
    }

    private unsetAttribute() {
        try {
            this.renderer.setProperty(this.target, 'title', '');
            return true;
        } catch  {
            return false;
        }
    }
}
