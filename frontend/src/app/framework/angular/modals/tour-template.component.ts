/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, signal, inject } from '@angular/core';
import { MarkdownDirective } from '../markdown.directive';
import { TranslatePipe } from '../pipes/translate.pipe';
import { ModalPlacementDirective } from './modal-placement.directive';
import { ModalDirective } from './modal.directive';
import { StepDefinition, TourService } from './tour.service';

@Component({
    selector: 'sqx-tour-template',
    styleUrls: ['./tour-template.component.scss'],
    templateUrl: './tour-template.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        MarkdownDirective,
        ModalDirective,
        ModalPlacementDirective,
        TranslatePipe,
    ],
})
export class TourTemplateComponent {
    private delayedDestroy: any;

    protected readonly tourService = inject(TourService);

    protected readonly isVisible = signal(false);

    protected readonly currentStep = signal<StepDefinition | null>(null);

    protected readonly currentElement = signal<any>(null);

    protected readonly hasPrev = false;

    protected readonly hasNext = computed(() => {
        const step = this.currentStep();
        if (!step) {
            return false;
        }

        return this.tourService.hasNext(step) && !step.nextOnAnchorClick && !step.nextOnCondition;
    });

    protected readonly hasFinish = computed(() => {
        const step = this.currentStep();
        if (!step) {
            return false;
        }

        return !this.tourService.hasNext(step) && !step.hideAll;
    });

    protected readonly progress = computed(() => {
        const step = this.currentStep();
        if (!step) {
            return '0%';
        }

        const stepIndex = this.tourService.steps.indexOf(step) + 1;
        const stepCount = this.tourService.steps.length;

        return `${100 * stepIndex / stepCount}%`;
    });

    constructor() {
        const tourService = this.tourService;

        tourService.component = this;
    }

    public render(step: StepDefinition | null, element: any | null) {
        this.isVisible.set(!!step);

        clearTimeout(this.delayedDestroy);

        if (!step) {
            this.delayedDestroy = setTimeout(() => {
                this.currentStep.set(null);
                this.currentElement.set(null);
            }, 4000);
        } else {
            this.currentStep.set(step);
            this.currentElement.set(element);
        }
    }
}
