/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { TourService as BaseTourService, IStepOption, TourAnchorDirective, TourState } from 'ngx-ui-tour-core';
import { filter, Observable, Subscription, take } from 'rxjs';
import { FloatingPlacement } from '@app/framework/internal';
import { TourTemplateComponent } from './tour-template.component';

export interface StepDefinition extends IStepOption {
    // The position.
    position?: FloatingPlacement;

    // Additional callback.
    hideAll?: () => void;

    // Additional callback.
    hideThis?: () => void;

    // Goes to the end automatically.
    endOnCondition?: ((service: TourService, anchor: TourAnchorDirective) => Observable<any>) | null;

    // Goes to the next element automatically.
    nextOnCondition?: ((service: TourService, anchor: TourAnchorDirective) => Observable<any>) | null;
}

export function waitForAnchor(anchorId: string) {
    return (service: TourService) => {
        service.unregister(anchorId);

        return service.anchorRegister$.pipe(
            filter(id => id === anchorId), take(1));
    };
}

export function waitForAnchorClick() {
    return (_: TourService, anchor: TourAnchorDirective) => {
        return new Observable<boolean>(subscriber => {
            const element = anchor.element.nativeElement as HTMLElement;

            const listener = () => {
                subscriber.next(true);
                subscriber.complete();
                element.removeEventListener('click', listener);
            };

            element.addEventListener('click', listener);

            return () => {
                element.removeEventListener('click', listener);
            };
        });
    };
}

export function waitForElement(selector: string) {
    return () => {
        return new Observable<boolean>(subscriber => {
            const observer = new MutationObserver((mutationsList) => {
                let shouldUpdate = false;

                for (const mutation of mutationsList) {
                    if (mutation.type === 'childList' || mutation.type === 'attributes') {
                        shouldUpdate = true;
                        break;
                    }
                }

                if (shouldUpdate) {
                    const element = document.querySelector(selector);
                    if (element) {
                        subscriber.next(true);
                        subscriber.complete();
                        observer.disconnect();
                    }
                }
            });

            observer.observe(document.body, {
                childList: true,
                subtree: true,
                attributes: true,
                attributeFilter: ['style', 'class'],
            });

            return () => {
                observer.disconnect();
            };
        });
    };
}

@Injectable({
    providedIn: 'root',
})
export class TourService extends BaseTourService<StepDefinition> {
    private onNext?: Subscription;
    private onEnd?: Subscription;

    public component?: TourTemplateComponent | null = null;

    constructor() {
        super();

        this.start$
            .subscribe(() => {
                document.body.style.overflow = 'hidden';
            });

        this.end$
            .subscribe(() => {
                document.body.style.overflow = 'auto';
            });

        this.stepShow$
            .subscribe(({ step }) => {
                const directive = this.anchors[step.anchorId!];

                this.onNext = step.nextOnCondition?.(this, directive)?.subscribe(() => {
                    this.goto(this.steps.indexOf(step) + 1);
                });

                this.onEnd = step.endOnCondition?.(this, directive)?.subscribe(() => {
                    this.end();
                });
            });

        this.stepHide$
            .subscribe(() => {
                if (this.getStatus() !== TourState.PAUSED) {
                    this.onNext?.unsubscribe();
                    this.onEnd?.unsubscribe();
                }
            });
    }

    public render(step: StepDefinition | null, target: any | null) {
        this.component?.render(step, target);
    }
}