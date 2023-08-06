/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, ElementRef, Input, OnDestroy, OnInit } from '@angular/core';
import { TourAnchorDirective } from 'ngx-ui-tour-core';
import { StepDefinition, TourService } from './tour.service';

@Directive({
    selector: '[sqxTourStep]',
})
export class TourStepDirective implements OnInit, OnDestroy, TourAnchorDirective {
    private isActive = false;

    @Input({ alias: 'sqxTourStep', required: true })
    public tourAnchor?: string | null;

    constructor(public readonly element: ElementRef,
        private readonly tourService: TourService,
    ) {
    }

    public ngOnInit(): void {
        if (!this.tourAnchor) {
            return;
        }

        this.tourService.register(this.tourAnchor, this);
    }

    public ngOnDestroy(): void {
        if (!this.tourAnchor) {
            return;
        }

        if (this.isActive) {
            this.tourService.render(null, null);
            this.tourService.pause();
        }

        this.tourService.unregister(this.tourAnchor);
    }

    public showTourStep(step: StepDefinition) {
        this.tourService.render(step, this.element.nativeElement);
        this.isActive = true;
    }

    public hideTourStep() {
        this.tourService.render(null, null);
        this.isActive = false;
    }
}
