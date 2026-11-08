/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { LanguagesState, LayoutComponent, ListViewComponent, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { LanguageAddFormComponent } from './language-add-form.component';
import { LanguageComponent } from './language.component';

@Component({
    selector: 'sqx-languages-page',
    styleUrls: ['./languages-page.component.scss'],
    templateUrl: './languages-page.component.html',
    imports: [
        LanguageAddFormComponent,
        LanguageComponent,
        LayoutComponent,
        ListViewComponent,
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
export class LanguagesPageComponent {
    protected readonly languagesState = inject(LanguagesState);

    constructor() {
        this.languagesState.load();
    }

    protected reload() {
        this.languagesState.load(true);
    }
}
