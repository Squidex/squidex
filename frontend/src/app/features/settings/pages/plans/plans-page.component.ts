/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ApiUrlConfig, ExternalLinkDirective, FormHintComponent, LayoutComponent, ListViewComponent, MarkdownPipe, NotifoComponent, PlansState, SafeHtmlPipe, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe, UserNamePipe } from '@app/shared';
import { PlanComponent } from './plan.component';

@Component({
    standalone: true,
    selector: 'sqx-plans-page',
    styleUrls: ['./plans-page.component.scss'],
    templateUrl: './plans-page.component.html',
    imports: [
        AsyncPipe,
        ExternalLinkDirective,
        FormHintComponent,
        LayoutComponent,
        ListViewComponent,
        MarkdownPipe,
        NotifoComponent,
        PlanComponent,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        SafeHtmlPipe,
        ShortcutDirective,
        SidebarMenuDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
        UserNamePipe,
    ],
})
export class PlansPageComponent implements OnInit {
    private overridePlanId?: string;

    public portalUrl = this.apiUrl.buildUrl('/portal/');

    constructor(
        public readonly plansState: PlansState,
        private readonly apiUrl: ApiUrlConfig,
        private readonly route: ActivatedRoute,
    ) {
    }

    public ngOnInit() {
        this.route.queryParams
            .subscribe(params => {
                this.overridePlanId = params['planId'];
            }).unsubscribe();

        this.plansState.load(false, this.overridePlanId);
    }

    public reload() {
        this.plansState.load(true, this.overridePlanId);
    }
}
