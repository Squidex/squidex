/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { NgTemplateOutlet } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { RuleTriggerMetadataDto, TranslatePipe } from '@app/shared';
import { RuleElementComponent } from './rule-element.component';

@Component({
    standalone: true,
    selector: 'sqx-history-step',
    styleUrls: ['./history-step.component.scss'],
    templateUrl: './history-step.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        NgTemplateOutlet,
        RuleElementComponent,
        TranslatePipe,
    ],
})
export class HistoryStepComponent {
    @Input({ transform: booleanAttribute })
    public isFirst = false;

    @Input({ transform: booleanAttribute })
    public isLast = false;

    @Input({ transform: booleanAttribute })
    public isExpandable = false;

    @Input({ transform: booleanAttribute })
    public isActive = false;

    @Input({ transform: booleanAttribute })
    public small = false;

    @Input()
    public elementType?: string;

    @Input()
    public elementInfo?: RuleTriggerMetadataDto;

    public isExpanded = false;

    public toggle() {
        this.isExpanded = !this.isExpanded;
    }
}