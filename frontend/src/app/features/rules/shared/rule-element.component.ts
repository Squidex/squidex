/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { ConfirmClickDirective, DarkenPipe, ExternalLinkDirective, RuleTriggerMetadataDto, StopClickDirective, TooltipDirective, TranslatePipe } from '@app/shared';
import { RuleIconComponent } from './rule-icon.component';

@Component({
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
    /** The type of the trigger or step, used as fallback title. */
    public readonly elementType = input.required<string>();

    /** The metadata of the trigger or step, with icon, title and description. */
    public readonly elementInfo = input.required<RuleTriggerMetadataDto>();

    /** The tooltip of the icon. Falls back to the title of the element. */
    public readonly label = input<string>();

    /** Shows the name next to the icon. */
    public readonly showName = input(true, { transform: booleanAttribute });

    /** Shows the description below the name. */
    public readonly showDescription = input(true, { transform: booleanAttribute });

    /** Shows a button to remove the element. */
    public readonly showRemove = input(false, { transform: booleanAttribute });

    /** Shows the element as not clickable. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** The size of the icon. */
    public readonly size = input<'sm' | 'md' | 'lg'>('lg');

    /** Emits when the remove button is clicked and confirmed. */
    public readonly remove = output();

    /** Emits when the icon is clicked. */
    public readonly iconClick = output();
}
