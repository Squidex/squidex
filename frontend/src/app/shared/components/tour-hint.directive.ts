/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/no-input-rename */
/* eslint-disable @angular-eslint/directive-selector */

import { Directive, effect, ElementRef, inject, input, numberAttribute } from '@angular/core';
import { timer } from 'rxjs';
import { FloatingPlacement, ModalService, StepDefinition, TourService, TourState } from '@app/shared/internal';

@Directive({
    selector: '[hintText]',
})
export class TourHintDirective {
    private readonly element = inject<ElementRef<Element>>(ElementRef);
    private readonly modalService = inject(ModalService);
    private readonly tourService = inject(TourService);
    private readonly tourState = inject(TourState);

    /** The ID of the tour step to show the hint for. */
    public readonly anchorId = input<string | undefined>(undefined, { alias: 'sqxTourStep' });

    /** The text of the hint. */
    public readonly hintText = input.required<string>();

    /** The time in milliseconds until the hint is shown. */
    public readonly hintAfter = input(1000, { transform: numberAttribute });

    /** The position of the hint. */
    public readonly hintPosition = input<FloatingPlacement>();

    /** The position of the hint, if no hint position is defined. */
    public readonly titlePosition = input<FloatingPlacement>();

    constructor() {
        effect(onCleanup => {
            const anchorId = this.anchorId();
            if (!anchorId) {
                return;
            }

            if (!this.tourState.shouldShowHint(anchorId)) {
                return;
            }

            const subscription = timer(this.hintAfter()).subscribe(() => {
                this.showHint(anchorId);
            });

            onCleanup(() => {
                subscription.unsubscribe();
            });
        });
    }

    private showHint(anchorId: string) {
        if (this.tourState.snapshot.status === 'Started') {
            return;
        }

        if (!this.tourState.shouldShowHint(anchorId)) {
            return;
        }

        // Dialogs and dropdowns would cover the hint, but keep the one that contains the anchor.
        this.modalService.hideAll(this.element.nativeElement);

        this.tourService.initialize([{
            anchorId,
            title: 'i18n:tour.hint',
            hideAll: () => this.hideAll(),
            hideThis: () => this.hideThis(anchorId),
            content: this.hintText(),
            nextOnAnchorClick: true,
            nextBtnTitle: undefined,
            position: this.hintPosition() || this.titlePosition(),
        } as StepDefinition]);
        this.tourService.start();

        this.tourState.disableHint(anchorId);
    }

    private hideThis(anchorId: string) {
        this.tourService.end();
        this.tourState.disableHint(anchorId);
    }

    private hideAll() {
        this.tourService.end();
        this.tourState.disableAllHints();
    }
}
