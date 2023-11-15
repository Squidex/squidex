/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ClientsState, FormAlertComponent, LayoutComponent, ListViewComponent, MarkdownInlinePipe, SafeHtmlPipe, ShortcutDirective, SidebarMenuDirective, TemplateDto, TemplatesState, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { TemplateComponent } from './template.component';

@Component({
    selector: 'sqx-templates-page',
    styleUrls: ['./templates-page.component.scss'],
    templateUrl: './templates-page.component.html',
    standalone: true,
    imports: [
        TitleComponent,
        LayoutComponent,
        TooltipDirective,
        ShortcutDirective,
        ListViewComponent,
        FormAlertComponent,
        NgIf,
        NgFor,
        TemplateComponent,
        SidebarMenuDirective,
        RouterLink,
        RouterLinkActive,
        TourStepDirective,
        RouterOutlet,
        AsyncPipe,
        MarkdownInlinePipe,
        SafeHtmlPipe,
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

    public trackByTemplate(_index: number, item: TemplateDto) {
        return item.name;
    }
}
