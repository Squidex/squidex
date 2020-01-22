/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: component-selector

import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

import { RuleElementDto } from '@app/shared';

@Component({
    selector: 'sqx-rule-icon',
    template: `
        <span class="icon {{size}}">
            <i *ngIf="element.iconCode; else svgIcon" class="icon icon-{{element.iconCode}}"></i>

            <ng-template #svgIcon>
                <i class="svg-icon" [innerHtml]="element.iconImage | sqxSafeHtml"></i>
            </ng-template>
        </span>
    `,
    styles: [`
        .svg-icon {
            display: inline-block;
        }

        .icon {
            color: white;
        }

        .sm .icon {
            font-size: 14px;
        }

        .sm .svg-icon {
            width: 14px;
        }

        .md .icon {
            font-size: 20px;
        }

        .md .svg-icon {
            width: 20px;
        }

        .lg .icon {
            font-size: 30px;
        }

        .lg .svg-icon {
            width: 30px;
        }

        ::ng-deep svg {
            fill: white;
            display: block;
        }`
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RuleIconComponent {
    @Input()
    public element: RuleElementDto;

    @Input()
    public size: 'sm' | 'md' | 'lg' = 'sm';
}