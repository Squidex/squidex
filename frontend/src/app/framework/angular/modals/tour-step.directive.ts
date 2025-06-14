/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, HostListener, Input, OnDestroy, OnInit } from '@angular/core';
import { TourAnchorDirective } from 'ngx-ui-tour-core';
import { StepDefinition, TourService } from './tour.service';

@Directive({
    selector: '[sqxTourStep]',
})
export class TourStepDirective implements OnInit, OnDestroy, TourAnchorDirective {
    private isNextOnClick = false;
    private isActive = false;
    private currentAnchorId?: string | null;
    private wasClicked = false;

    @Input({ alias: 'sqxTourStep', required: true })
    public anchorId?: string | null;

    constructor(public readonly element: ElementRef,
        private readonly tourService: TourService,
    ) {
    }

    public ngOnInit(): void {
        this.currentAnchorId = this.anchorId;

        if (!this.currentAnchorId) {
            return;
        }

        this.tourService.register(this.currentAnchorId, this);
    }

    public ngOnDestroy(): void {
        if (!this.currentAnchorId) {
            return;
        }

        if (this.isActive && (!this.isNextOnClick || !this.wasClicked)) {
            setTimeout(() => {
                if (this.tourService.currentStep.anchorId === this.currentAnchorId) {
                    this.tourService.render(null, null);
                    this.tourService.pause();
                }
            }, 200);
        }

        this.tourService.unregister(this.currentAnchorId);
    }

    @HostListener('click')
    public onClick() {
        this.wasClicked = true;
    }

    public showTourStep(step: StepDefinition) {
        this.tourService.render(step, this.element.nativeElement);
        this.isActive = true;
        this.isNextOnClick = !!step.nextOnAnchorClick;
    }

    public hideTourStep() {
        this.tourService.render(null, null);
        this.isActive = false;
        this.isNextOnClick = false;
    }
}
