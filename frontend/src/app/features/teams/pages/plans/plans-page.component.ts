/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ActivatedRoute, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ApiUrlConfig, ExternalLinkDirective, FormHintComponent, LayoutComponent, ListViewComponent, MarkdownDirective, NotifoComponent, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { TeamPlansState } from '../../internal';
import { PlanComponent } from './plan.component';

@Component({
    selector: 'sqx-plans-page',
    styleUrls: ['./plans-page.component.scss'],
    templateUrl: './plans-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
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

    public readonly portalUrl;

    constructor(apiUrl: ApiUrlConfig, route: ActivatedRoute,
        public readonly plansState: TeamPlansState,
    ) {
        this.portalUrl = apiUrl.buildUrl('/portal/');

        this.overridePlanId = route.snapshot.queryParams['planId'];

        this.plansState.load(false, this.overridePlanId);
    }

    public reload() {
        this.plansState.load(true, this.overridePlanId);
    }
}
