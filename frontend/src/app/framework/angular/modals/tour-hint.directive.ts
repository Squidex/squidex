/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */
/*

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Directive, ElementRef, Input, OnDestroy, OnInit, Renderer2 } from '@angular/core';
import { timer } from 'rxjs';
import { DialogModel, fadeAnimation, OnboardingService, RelativePosition, StatefulComponent, Types } from '@app/framework/internal';
import { TourService } from 'ngx-ui-tour-core';

@Directive({
    selector: 'sqxTourHint',
})
export class TourHintDirective extends StatefulComponent implements OnDestroy, OnInit {
    @Input()
    public for: any;

    @Input()
    public helpId = '';

    @Input()
    public after = 1000;

    @Input()
    public position: RelativePosition = 'left-center';

    public tooltipModal = new DialogModel();

    constructor(
        private readonly element: ElementRef,
        private readonly tourService: TourService,
        private readonly onboardingService: OnboardingService,
        private readonly renderer: Renderer2,
    ) {
        super(changeDetector, {});
    }

    public ngOnDestroy() {
        super.ngOnDestroy();

        this.tooltipModal.hide();
    }

    public ngOnInit() {
        if (!this.helpId || !Types.isFunction(this.for?.addEventListener)) {
            return;
        }

        this.own(
            timer(this.after).subscribe(() => {
                if (this.onboardingService.shouldShow(this.helpId)) {
                    const forRect = this.for.getBoundingClientRect();

                    const x = forRect.left + 0.5 * forRect.width;
                    const y = forRect.top + 0.5 * forRect.height;

                    const fromPoint = document.elementFromPoint(x, y);

                    if (this.isSameOrParent(fromPoint)) {
                        this.tooltipModal.show();

                        this.own(
                            timer(10000).subscribe(() => {
                                this.hideThis();
                            }));

                        this.onboardingService.disable(this.helpId);
                    }
                }
            }));

        this.own(
            this.renderer.listen(this.for, 'mousedown', () => {
                this.onboardingService.disable(this.helpId);

                this.hideThis();
            }));
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
        this.onboardingService.disable(this.helpId);

        this.ngOnDestroy();
    }

    public hideAll() {
        this.onboardingService.disableAll();

        this.ngOnDestroy();
    }
}
*/