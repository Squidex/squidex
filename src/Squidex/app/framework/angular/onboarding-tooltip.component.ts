/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input, OnDestroy, OnInit, Renderer } from '@angular/core';

import { ModalView } from './../utils/modal-view';

import { OnboardingService } from './../services/onboarding.service';

@Component({
    selector: 'sqx-onboarding-tooltip',
    styleUrls: ['./onboarding-tooltip.component.scss'],
    templateUrl: './onboarding-tooltip.component.html'
})
export class OnboardingTooltipComponent implements OnDestroy, OnInit {
    private showTimer: any;
    private closeTimer: any;
    private forClickListener: Function;

    public tooltipModal = new ModalView();

    @Input()
    public for: any;

    @Input()
    public id: string;

    @Input()
    public position = 'left';

    @Input()
    public after = 1000;

    @Input()
    public text: string;

    constructor(
        private readonly onboardingService: OnboardingService,
        private readonly renderer: Renderer
    ) {
    }

    public ngOnDestroy() {
        if (this.showTimer) {
            clearTimeout(this.showTimer);
            this.showTimer = null;
        }

        if (this.closeTimer) {
            clearTimeout(this.closeTimer);
            this.closeTimer = null;
        }

        if (this.forClickListener) {
            this.forClickListener();
            this.forClickListener = null;
        }
    }

    public ngOnInit() {
        if (this.for && this.id) {
            this.showTimer = setTimeout(() => {
                if (this.onboardingService.shouldShow(this.id)) {
                    this.tooltipModal.show();

                    this.closeTimer = setTimeout(() => {
                        this.hideThis();
                    }, 10000);
                }
            }, this.after);

            this.forClickListener =
                this.renderer.listen(this.for, 'mousedown', () => {
                    this.onboardingService.disable(this.id);

                    this.hideThis();
                });
        }
    }

    public hideThis() {
        this.onboardingService.disable(this.id);
        this.tooltipModal.hide();

        this.ngOnDestroy();
    }

    public hideAll() {
        this.onboardingService.disableAll();
        this.tooltipModal.hide();

        this.ngOnDestroy();
    }
}