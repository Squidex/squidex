/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { NgTemplateOutlet } from '@angular/common';
import { booleanAttribute, Component, input, signal } from '@angular/core';
import { RuleTriggerMetadataDto, TranslatePipe } from '@app/shared';
import { RuleElementComponent } from './rule-element.component';

@Component({
    selector: 'sqx-history-step',
    styleUrls: ['./history-step.component.scss'],
    templateUrl: './history-step.component.html',
    imports: [
        NgTemplateOutlet,
        RuleElementComponent,
        TranslatePipe,
    ],
})
export class HistoryStepComponent {
    /** Marks the step as the first step in the history. */
    public readonly isFirst = input(false, { transform: booleanAttribute });

    /** Marks the step as the last step and hides the connecting line. */
    public readonly isLast = input(false, { transform: booleanAttribute });

    /** Allows the user to expand the step to show the details. */
    public readonly isExpandable = input(false, { transform: booleanAttribute });

    /** Highlights the step as the current step. */
    public readonly isActive = input(false, { transform: booleanAttribute });

    /** Renders the step in a compact layout. */
    public readonly small = input(false, { transform: booleanAttribute });

    /** The type of the trigger or step to show the icon for. */
    public readonly elementType = input<string>();

    /** The metadata of the trigger or step to show the icon for. */
    public readonly elementInfo = input<RuleTriggerMetadataDto>();

    protected readonly isExpanded = signal(false);

    protected toggle() {
        this.isExpanded.update(x => !x);
    }
}
