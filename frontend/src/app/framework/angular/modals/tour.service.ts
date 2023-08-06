/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { TourService as BaseTourService, IStepOption } from 'ngx-ui-tour-core';
import { filter, Observable, Subscription, take } from 'rxjs';
import { RelativePosition } from '@app/framework/internal';
import { TourTemplateComponent } from './tour-template.component';

export interface StepDefinition extends IStepOption {
    // The position.
    position?: RelativePosition;

    // Goes to the next element automatically.
    nextOnCondition?: ((service: TourService) => Observable<any>) | null;
}

export function waitForAnchor(anchorId: string) {
    return (service: TourService) => {
        service.unregister(anchorId);

        return service.anchorRegister$.pipe(
            filter(id => id === anchorId), take(1));
    };
}

@Injectable()
export class TourService extends BaseTourService<StepDefinition> {
    private condition?: Subscription;

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

        this.stepHide$
            .subscribe(() => {
                this.condition?.unsubscribe();
            });
    }

    public render(step: StepDefinition | null, target: any | null) {
        this.component?.render(step, target);
    }

    protected showStep(step: StepDefinition): Promise<void> {
        this.condition = step.nextOnCondition?.(this)?.subscribe(() => {
            this.next();
        });

        return super.showStep(step);
    }
}