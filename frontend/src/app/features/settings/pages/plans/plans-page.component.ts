/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ExternalLinkDirective, FormHintComponent, LayoutComponent, ListViewComponent, MarkdownDirective, NotifoComponent, PlansState, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe, UserNamePipe } from '@app/shared';
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
        UserNamePipe,
    ],
})
export class PlansPageComponent {
    private readonly route = inject(ActivatedRoute);
    private readonly overridePlanId: string | undefined = this.route.snapshot.queryParams['planId'];

    protected readonly plansState = inject(PlansState);

    constructor() {
        this.plansState.load(false, this.overridePlanId);
    }

    protected reload() {
        this.plansState.load(true, this.overridePlanId);
    }
}
