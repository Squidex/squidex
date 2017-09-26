/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input, OnDestroy, OnInit, Renderer } from '@angular/core';

import { ModalView } from './../utils/modal-view';
import { Types } from './../utils/types';

import { OnboardingService } from './../services/onboarding.service';

import { fadeAnimation } from './animations';

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
    private forMouseDownListener: Function;

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
        private readonly renderer: Renderer
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
                    this.tooltipModal.show();

                    this.closeTimer = setTimeout(() => {
                        this.hideThis();
                    }, 10000);

                    this.onboardingService.disable(this.id);
                }
            }, this.after);

            this.forMouseDownListener =
                this.renderer.listen(this.for, 'mousedown', () => {
                    this.onboardingService.disable(this.id);

                    this.hideThis();
                });
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