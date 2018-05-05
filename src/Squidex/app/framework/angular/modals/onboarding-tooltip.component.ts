/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, Input, OnDestroy, OnInit, Renderer2 } from '@angular/core';

import {
    fadeAnimation,
    ModalView,
    OnboardingService,
    Types
} from '@app/framework/internal';

@Component({
    selector: 'sqx-onboarding-tooltip',
    styleUrls: ['./onboarding-tooltip.component.scss'],
    templateUrl: './onboarding-tooltip.component.html',
    animations: [
        fadeAnimation
    ]
})
export class OnboardingTooltipComponent implements OnDestroy, OnInit {
    private showTimer: any;
    private closeTimer: any;
    private forMouseDownListener: Function | null;

    public tooltipModal = new ModalView();

    @Input()
    public for: any;

    @Input()
    public id: string;

    @Input()
    public after = 1000;

    @Input()
    public position = 'left';

    constructor(
        private readonly onboardingService: OnboardingService,
        private readonly renderer: Renderer2
    ) {
    }

    public ngOnDestroy() {
        clearTimeout(this.showTimer);
        clearTimeout(this.closeTimer);

        this.tooltipModal.hide();

        if (this.forMouseDownListener) {
            this.forMouseDownListener();
            this.forMouseDownListener = null;
        }
    }

    public ngOnInit() {
        if (this.for && this.id && Types.isFunction(this.for.addEventListener)) {
            this.showTimer = setTimeout(() => {
                if (this.onboardingService.shouldShow(this.id)) {
                    const forRect = this.for.getBoundingClientRect();

                    const x = forRect.left + 0.5 * forRect.width;
                    const y = forRect.top  + 0.5 * forRect.height;

                    const fromPoint = document.elementFromPoint(x, y);

                    if (this.isSameOrParent(fromPoint)) {
                        this.tooltipModal.show();

                        this.closeTimer = setTimeout(() => {
                            this.hideThis();
                        }, 10000);

                        this.onboardingService.disable(this.id);
                    }
                }
            }, this.after);

            this.forMouseDownListener =
                this.renderer.listen(this.for, 'mousedown', () => {
                    this.onboardingService.disable(this.id);

                    this.hideThis();
                });
        }
    }

    private isSameOrParent(underCursor: Element | null): boolean {
        if (!underCursor) {
            return false;
        } if (this.for === underCursor) {
            return true;
        } else {
            return this.isSameOrParent(this.renderer.parentNode(underCursor));
        }
    }

    public hideThis() {
        this.onboardingService.disable(this.id);

        this.ngOnDestroy();
    }

    public hideAll() {
        this.onboardingService.disableAll();

        this.ngOnDestroy();
    }
}