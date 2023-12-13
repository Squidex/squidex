/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/no-input-rename */
/* eslint-disable @angular-eslint/directive-selector */

import { Directive, Input, numberAttribute, OnInit } from '@angular/core';
import { timer } from 'rxjs';
import { FloatingPlacement, StepDefinition, Subscriptions, TourService, TourState, Types } from '@app/shared/internal';

@Directive({
    selector: '[hintText]',
    standalone: true,
})
export class TourHintDirective implements OnInit {
    private readonly subscriptions = new Subscriptions();

    @Input('sqxTourStep')
    public anchorId!: string;

    @Input()
    public hintText!: string;

    @Input({ transform: numberAttribute })
    public hintAfter: number = 1000;

    @Input()
    public hintPosition?: FloatingPlacement;

    @Input()
    public titlePosition?: FloatingPlacement;

    constructor(
        private readonly tourService: TourService,
        private readonly tourState: TourState,
    ) {
    }

    public ngOnDestroy() {
        return;
    }

    public ngOnInit() {
        if (!this.anchorId) {
            return;
        }

        if (!this.tourState.shouldShowHint(this.anchorId)) {
            return;
        }

        const after =
            Types.isNumber(this.hintAfter) ?
            this.hintAfter :
            parseInt(this.hintAfter, 10);

        this.subscriptions.add(
            timer(after).subscribe(() => {
                if (this.tourState.snapshot.status === 'Started') {
                    return;
                }

                if (!this.tourState.shouldShowHint(this.anchorId)) {
                    return;
                }

                this.tourService.initialize([{
                    anchorId: this.anchorId,
                    title: 'i18n:tour.hint',
                    hideAll: () => this.hideAll(),
                    hideThis: () => this.hideThis(),
                    content: this.hintText,
                    nextOnAnchorClick: true,
                    nextBtnTitle: undefined,
                    position: this.hintPosition || this.titlePosition,
                } as StepDefinition]);
                this.tourService.start();

                this.tourState.disableHint(this.anchorId);
            }));
    }

    private hideThis(): void {
        this.tourService.end();
        this.tourState.disableHint(this.anchorId);
    }

    private hideAll(): void {
        this.tourService.end();
        this.tourState.disableAllHints();
    }
}
