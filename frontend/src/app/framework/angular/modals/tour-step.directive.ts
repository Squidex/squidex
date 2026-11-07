/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, effect, ElementRef, inject, input, untracked } from '@angular/core';
import { TourAnchorDirective } from 'ngx-ui-tour-core';
import { StepDefinition, TourService } from './tour.service';

@Directive({
    selector: '[sqxTourStep]',
    host: {
        '(click)': 'onClick()',
    },
})
export class TourStepDirective implements TourAnchorDirective {
    private readonly tourService = inject(TourService);
    private isActive = false;
    private isNextOnClick = false;
    private wasClicked = false;

    public readonly element = inject(ElementRef);

    /** The ID of the tour anchor to attach this element to. */
    public readonly anchorId = input.required<string | undefined | null>({ alias: 'sqxTourStep' });

    constructor() {
        effect(onCleanup => {
            const anchorId = this.anchorId();
            if (!anchorId) {
                return;
            }

            untracked(() => this.tourService.register(anchorId, this));

            onCleanup(() => this.unregister(anchorId));
        });
    }

    protected onClick() {
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

    private unregister(anchorId: string) {
        if (this.isActive && (!this.isNextOnClick || !this.wasClicked)) {
            setTimeout(() => {
                if (this.tourService.currentStep.anchorId === anchorId) {
                    this.tourService.render(null, null);
                    this.tourService.pause();
                }
            }, 200);
        }

        this.tourService.unregister(anchorId);
    }
}
