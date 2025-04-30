/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { ConfirmClickDirective, DarkenPipe, ExternalLinkDirective, RuleTriggerMetadataDto, StopClickDirective, TooltipDirective, TranslatePipe } from '@app/shared';
import { RuleIconComponent } from './rule-icon.component';

@Component({
    standalone: true,
    selector: 'sqx-rule-element',
    styleUrls: ['./rule-element.component.scss'],
    templateUrl: './rule-element.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        DarkenPipe,
        ExternalLinkDirective,
        RuleIconComponent,
        StopClickDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class RuleElementComponent {
    @Input({ required: true })
    public elementType!: string;

    @Input({ required: true })
    public elementInfo!: RuleTriggerMetadataDto;

    @Input()
    public label?: string;

    @Input({ transform: booleanAttribute })
    public showName?: boolean | null = true;

    @Input({ transform: booleanAttribute })
    public showDescription?: boolean | null = true;

    @Input({ transform: booleanAttribute })
    public showRemove?: boolean | null = false;

    @Input({ transform: booleanAttribute })
    public disabled = false;

    @Output()
    public remove = new EventEmitter();

    @Output()
    public iconClick = new EventEmitter();
}
