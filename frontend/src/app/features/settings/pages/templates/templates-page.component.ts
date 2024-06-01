/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ClientsState, FormAlertComponent, LayoutComponent, ListViewComponent, MarkdownInlinePipe, SafeHtmlPipe, ShortcutDirective, SidebarMenuDirective, TemplatesState, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { TemplateComponent } from './template.component';

@Component({
    standalone: true,
    selector: 'sqx-templates-page',
    styleUrls: ['./templates-page.component.scss'],
    templateUrl: './templates-page.component.html',
    imports: [
        AsyncPipe,
        FormAlertComponent,
        LayoutComponent,
        ListViewComponent,
        MarkdownInlinePipe,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        SafeHtmlPipe,
        ShortcutDirective,
        SidebarMenuDirective,
        TemplateComponent,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class TemplatesPageComponent implements OnInit {
    constructor(
        public readonly clientsState: ClientsState,
        public readonly templatesState: TemplatesState,
    ) {
    }

    public ngOnInit() {
        this.clientsState.load();

        this.templatesState.load();
    }

    public reload() {
        this.templatesState.load(true);
    }
}
