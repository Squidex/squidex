/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { AfterContentInit, ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { fadeAnimation, StatefulComponent } from '@app/framework/internal';
import { MarkdownInlinePipe, MarkdownPipe } from '../pipes/markdown.pipe';
import { TranslatePipe } from '../pipes/translate.pipe';
import { SafeHtmlPipe } from '../safe-html.pipe';
import { ModalPlacementDirective } from './modal-placement.directive';
import { ModalDirective } from './modal.directive';
import { StepDefinition, TourService } from './tour.service';

@Component({
    standalone: true,
    selector: 'sqx-tour-template',
    styleUrls: ['./tour-template.component.scss'],
    templateUrl: './tour-template.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        MarkdownInlinePipe,
        MarkdownPipe,
        ModalDirective,
        ModalPlacementDirective,
        SafeHtmlPipe,
        TranslatePipe,
    ],
})
export class TourTemplateComponent extends StatefulComponent implements AfterContentInit {
    private delayedDestory: any;

    @Input()
    public currentStep!: StepDefinition;

    @Input()
    public currentElement?: any = null;

    public isVisible = false;

    public hasPrev = false;
    public hasNext = false;
    public hasFinish = false;

    public progress = '0%';

    constructor(
        public readonly tourService: TourService,
    ) {
        super({});
    }

    public ngAfterContentInit() {
        this.tourService.component = this;
    }

    public render(step: StepDefinition | null, element: any | null) {
        this.isVisible = !!step;

        clearTimeout(this.delayedDestory);

        if (!step) {
            this.delayedDestory = setTimeout(() => {
                this.currentStep = null!;
                this.currentElement = null;
                this.detectChanges();
            }, 4000);
        } else {
            const stepIndex = this.tourService.steps.indexOf(step) + 1;
            const stepCount = this.tourService.steps.length;

            this.progress = `${100 * stepIndex / stepCount}%`;

            this.hasPrev = false;
            this.hasNext = this.tourService.hasNext(step) && !step.nextOnAnchorClick && !step.nextOnCondition;
            this.hasFinish = !this.tourService.hasNext(step) && !step.hideAll;

            this.currentStep = step;
            this.currentElement = element;
        }

        this.detectChanges();
    }
}
