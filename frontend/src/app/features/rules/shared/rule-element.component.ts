/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { DarkenPipe, ExternalLinkDirective, HoverBackgroundDirective, RuleElementMetadataDto, StopClickDirective, TranslatePipe } from '@app/shared';
import { RuleIconComponent } from './rule-icon.component';

@Component({
    standalone: true,
    selector: 'sqx-rule-element',
    styleUrls: ['./rule-element.component.scss'],
    templateUrl: './rule-element.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DarkenPipe,
        ExternalLinkDirective,
        HoverBackgroundDirective,
        RuleIconComponent,
        StopClickDirective,
        TranslatePipe,
    ],
})
export class RuleElementComponent {
    @Input({ required: true })
    public type!: string;

    @Input({ required: true })
    public element!: RuleElementMetadataDto;

    @Input({ transform: booleanAttribute })
    public isSmall?: boolean | null = true;

    @Input({ transform: booleanAttribute })
    public disabled = false;
}
