/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */
/* eslint-disable @angular-eslint/no-output-native */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ConfirmClickDirective, FlowView, FromNowPipe, RuleElementDto, RuleEventDto, TranslatePipe } from '@app/shared';
import { RuleClassPipe } from '../../shared/pipes';
import { RuleElementComponent } from '../../shared/rule-element.component';
import { StateDetailsComponent } from '../../shared/state-details.component';

@Component({
    selector: '[sqxRuleEvent]',
    styleUrls: ['./rule-event.component.scss'],
    templateUrl: './rule-event.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        FormsModule,
        FromNowPipe,
        RuleClassPipe,
        RuleElementComponent,
        StateDetailsComponent,
        TranslatePipe,
    ],
})
export class RuleEventComponent {
    /** The rule event to show. */
    public readonly event = input.required<RuleEventDto>({ alias: 'sqxRuleEvent' });

    /** The available steps by type, to resolve the metadata of each step. */
    public readonly availableSteps = input.required<Record<string, RuleElementDto>>();

    /** Shows the details of the event. */
    public readonly expanded = input(false, { transform: booleanAttribute });

    /** Emits when the user wants to toggle the details. */
    public readonly expandedChange = output();

    /** Emits when the user wants to enqueue the event again. */
    public readonly enqueue = output();

    /** Emits when the user confirms to cancel the event. */
    public readonly cancel = output();

    protected readonly branchItems = computed(() => {
        return new FlowView(this.event().flowState.definition as any).getAllItems();
    });
}
