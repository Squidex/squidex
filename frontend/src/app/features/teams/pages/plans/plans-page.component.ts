/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ExternalLinkDirective, FormHintComponent, LayoutComponent, ListViewComponent, MarkdownDirective, NotifoComponent, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { TeamPlansState } from '../../internal';
import { PlanComponent } from './plan.component';

@Component({
    selector: 'sqx-plans-page',
    styleUrls: ['./plans-page.component.scss'],
    templateUrl: './plans-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ExternalLinkDirective,
        FormHintComponent,
        LayoutComponent,
        ListViewComponent,
        MarkdownDirective,
        NotifoComponent,
        PlanComponent,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        ShortcutDirective,
        SidebarMenuDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class PlansPageComponent {
    private readonly overridePlanId?: string;

    protected readonly plansState = inject(TeamPlansState);

    constructor() {
        const route = inject(ActivatedRoute);

        this.overridePlanId = route.snapshot.queryParams['planId'];

        this.plansState.load(false, this.overridePlanId);
    }

    protected reload() {
        this.plansState.load(true, this.overridePlanId);
    }
}
