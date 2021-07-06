/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnDestroy, OnInit, Renderer2 } from '@angular/core';
import { DialogModel, fadeAnimation, OnboardingService, StatefulComponent, Types } from '@app/framework/internal';
import { timer } from 'rxjs';

@Component({
    selector: 'sqx-onboarding-tooltip',
    styleUrls: ['./onboarding-tooltip.component.scss'],
    templateUrl: './onboarding-tooltip.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OnboardingTooltipComponent extends StatefulComponent implements OnDestroy, OnInit {
    @Input()
    public for: any;

    @Input()
    public helpId: string;

    @Input()
    public after = 1000;

    @Input()
    public position = 'left';

    public tooltipModal = new DialogModel();

    constructor(changeDetector: ChangeDetectorRef,
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
        if (this.for && this.helpId && Types.isFunction(this.for.addEventListener)) {
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
