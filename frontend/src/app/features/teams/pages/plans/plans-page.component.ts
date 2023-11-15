/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ApiUrlConfig, ExternalLinkDirective, FormHintComponent, LayoutComponent, ListViewComponent, MarkdownPipe, NotifoComponent, PlanDto, SafeHtmlPipe, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { TeamPlansState } from '../../internal';
import { PlanComponent } from './plan.component';

@Component({
    selector: 'sqx-plans-page',
    styleUrls: ['./plans-page.component.scss'],
    templateUrl: './plans-page.component.html',
    standalone: true,
    imports: [
        TitleComponent,
        LayoutComponent,
        NotifoComponent,
        TooltipDirective,
        ShortcutDirective,
        ListViewComponent,
        NgIf,
        FormHintComponent,
        NgFor,
        PlanComponent,
        ExternalLinkDirective,
        SidebarMenuDirective,
        RouterLink,
        RouterLinkActive,
        TourStepDirective,
        RouterOutlet,
        AsyncPipe,
        MarkdownPipe,
        SafeHtmlPipe,
        TranslatePipe,
    ],
})
export class PlansPageComponent implements OnInit {
    private overridePlanId?: string;

    public portalUrl = this.apiUrl.buildUrl('/portal/');

    constructor(
        public readonly plansState: TeamPlansState,
        private readonly apiUrl: ApiUrlConfig,
        private readonly route: ActivatedRoute,
    ) {
    }

    public ngOnInit() {
        this.overridePlanId = this.route.snapshot.queryParams['planId'];

        this.plansState.load(false, this.overridePlanId);
    }

    public reload() {
        this.plansState.load(true, this.overridePlanId);
    }

    public trackByPlan(_index: number, planInfo: { plan: PlanDto }) {
        return planInfo.plan.id;
    }
}
